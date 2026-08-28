# ColumbusSync.BranchBC

B지점/C지점(TS2020, `TSDB.mdb`) 데이터를 통합 허브 DB(`COLUMBUS_WEIGH_HUB`)로 옮기는
**독립 실행 콘솔 앱**입니다. A지점용 `ColumbusSync.BranchA`와 같은 역할이지만, 원본이
SQL Server가 아니라 Access mdb 파일이라는 점이 다릅니다.

## 어떻게 배포하나요

**B지점 PC와 C지점 PC 양쪽에 완전히 같은 코드를 설치합니다.** 두 PC의 차이는 오직
`App.config`의 두 값뿐입니다.

| 설정 | B지점 PC | C지점 PC |
|---|---|---|
| `BranchCode` | `B` | `C` |
| `MdbFilePath` | B지점 TS2020의 실제 `TSDB.mdb` 경로 | C지점 TS2020의 실제 `TSDB.mdb` 경로 |

즉, 이 프로젝트를 한 번 빌드해서 두 PC에 각각 복사하고, `App.config`만 다르게 채우면 됩니다.

## 독립성

- 메인 계량 화면 프로젝트(`ColumbusWeighing`)나 `ColumbusSync.BranchA`를 전혀 참조하지 않습니다.
- 빌드 결과(`bin\x86\Release\` 또는 `bin\x64\Release\`의 `ColumbusSync.BranchBC.exe` +
  설정파일)만 그대로 복사하면 실행 가능합니다. 다만 비트수를 신경 쓰지 않으려면
  `ColumbusSync.BranchBC.Launcher`를 통해 배포하는 걸 권장합니다(아래 참고).
- TS2020이 쓰는 mdb를 **읽기 전용**으로만 조회합니다. TS2020 프로그램은 지금처럼 그대로
  계속 쓰시면 되고, 이 프로그램이 mdb에 뭔가를 쓰는 일은 없습니다.
- 화면(UI) 없는 콘솔 프로그램이며, 실행하면 내부 타이머로 계속 대기하다가 설정된 주기마다
  스스로 동기화를 수행합니다.

## 실행 전 설정

1. `App.config.example`을 같은 폴더에 `App.config`로 복사합니다.
2. `App.config`를 열어 다음을 채웁니다.
   - `connectionStrings`의 `IntegrationHub` — 통합 허브 DB 접속정보(`121.66.17.30,16433` /
     `COLUMBUS_WEIGH_HUB`, 계정 `pineit_ex`)의 비밀번호.
   - `appSettings`의 `BranchCode` — 이 PC가 담당하는 지점(`B` 또는 `C`).
   - `appSettings`의 `MdbFilePath` — 이 PC의 실제 `TSDB.mdb` 전체 경로.

`App.config`는 Git 추적 대상이 아닙니다(`.gitignore`에 등록됨) — 실제 값을 채워도
커밋/푸시될 걱정이 없습니다. 자세한 배경은 `ColumbusSync.BranchA/README.md`의
"비밀번호 보호" 관련 설명을 참고하세요(같은 구조입니다).

### 필수 사전 설치: Microsoft Access Database Engine

mdb 파일을 읽으려면 **ACE OLEDB 12.0 드라이버**(Microsoft Access Database Engine
Redistributable)가 그 PC에 설치되어 있어야 합니다. TS2020(Access 기반 프로그램)이 이미
설치되어 있는 PC라면 대개 이미 깔려 있을 가능성이 높지만, 없다면 별도로 설치해야 합니다.

**비트수(32비트/64비트)를 이 워커 exe와 정확히 맞춰야 합니다.** PC마다 32비트가 깔려있을
수도, 64비트가 깔려있을 수도 있어서, 직접 이 프로젝트만 빌드해서 배포하면 매번 그 PC의
비트수를 확인해야 하는 번거로움이 있습니다.

**→ 그래서 직접 이 exe를 실행하지 마시고, `../ColumbusSync.BranchBC.Launcher`를
   대신 사용하는 걸 권장합니다.** 그 런처가 PC에 설치된 드라이버 비트수를 자동으로
   판단해서 맞는 쪽(x86/x64)을 대신 실행해줍니다. 배포 폴더 구성과 패키징 방법은
   그 프로젝트의 README를 참고하세요.

(직접 이 프로젝트만 빌드해서 쓰고 싶다면, `ColumbusSync.BranchBC.csproj`를
`msbuild ... /p:Platform=x86` 또는 `/p:Platform=x64`로 빌드해 그 PC와 비트수를 맞추면 됩니다.)

## 구현 상태

- 컬럼명은 첨부받은 `TSDB.mdb` 파일의 실제 스키마(mdb-schema로 직접 확인)를 기준으로
  했습니다. A지점처럼 화면 소스코드를 거쳐 추정한 게 아니라 mdb 구조 자체를 읽은 것이라
  신뢰도가 높습니다.
- 거래처(`TB_CUSTO`), 차량(`TB_CAR`), 품목(`TB_PUM`), 계근(`TB_WEIGH`) 전부 구현되어
  있습니다.
- A지점과 달리 차량의 운전자명/공차중량, 품목의 감량중량/감량율이 실제로 존재하는
  컬럼이라 그대로 채워집니다 (A지점 MES에는 없는 컬럼이라 항상 비어있었던 부분).
- 입/출고 구분(`IN_OUT_TYPE`), 완료 여부(`IS_COMPLETED`)는 A지점과 동일한 규칙(1·2차
  중량 비교, 2차중량 존재 여부)으로 재계산합니다 — mdb의 `INOUT`/`WEIGH_STS` 원본 값은
  참고용(`SOURCE_RAW_STATUS`)으로만 보존하고 그대로 믿지 않습니다.
- 계근 조회 기간은 `WeighSyncLookbackDays`(기본 30일)로 조정 가능합니다. A지점과 같은
  이유로, 검수/수정이 이 기간보다 늦게 들어오면 반영되지 않습니다.

## 빌드

Visual Studio 2017+ 또는 `msbuild`로 `ColumbusSync.BranchBC.csproj`를 빌드하면 됩니다.
DevExpress 등 외부 컴포넌트는 필요 없지만, 위에서 설명한 ACE OLEDB 드라이버는
**실행하는 PC**(빌드하는 PC가 아니라)에 설치되어 있어야 합니다.
