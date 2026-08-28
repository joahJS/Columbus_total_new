# ColumbusSync.BranchA

A지점(Columbus_total, 기존 MES) 데이터를 통합 허브 DB(`COLUMBUS_WEIGH_HUB`)로 옮기는
**독립 실행 콘솔 앱**입니다. B/C지점의 mdb 10분 자동 복사 프로그램과 같은 역할을 A지점 몫으로
수행합니다.

## 독립성

- 메인 계량 화면 프로젝트(`ColumbusWeighing`)나 DevExpress를 전혀 참조하지 않습니다.
- 이 폴더(빌드 결과 `bin\Release\ColumbusSync.BranchA.exe` + `App.config`)만 그대로
  다른 PC에 복사해서 실행하면 됩니다. Visual Studio나 다른 프로젝트가 없어도 동작합니다.
- 화면(UI)이 없는 콘솔 프로그램이며, 실행하면 내부 타이머로 계속 대기하다가 설정된 주기마다
  스스로 동기화를 수행합니다. 창을 최소화해두거나, 원한다면 이후 Windows 서비스/작업 스케줄러
  등록으로 바꿀 수 있습니다.

## 실행 전 설정 (`App.config`)

**`App.config`는 Git 추적 대상이 아닙니다.** (비밀번호가 실수로 커밋되는 사고가 있었고,
그 뒤로 `git update-index --skip-worktree`로 막아봤지만, 템플릿 자체가 바뀔 때마다
`git pull`이 "로컬 변경사항이 덮어써진다"며 막히는 문제가 있어서, 아예 추적 대상에서
뺐습니다.)

**처음 받으신 분은 다음과 같이 설정하세요.**

1. 같은 폴더의 `App.config.example`을 복사해서 `App.config`라는 이름으로 저장합니다.
2. `App.config`를 열어 `connectionStrings` 두 개를 실제 값으로 채웁니다.

| 이름 | 용도 | 현재 알려진 서버 정보 |
|---|---|---|
| `BranchA_Mes` | A지점 MES 원본 DB | `sql16ssd-006.localnet.kr,1433` / DB `columbusdb_pineit` |
| `IntegrationHub` | 통합 허브 DB | `121.66.17.30,16433` / DB `COLUMBUS_WEIGH_HUB` |

`App.config`는 `.gitignore`에 등록되어 있어서, 실제 값을 채워도 커밋/푸시될 일이 없고
`git pull`도 더 이상 이 파일 때문에 막히지 않습니다.

`appSettings`의 `SyncIntervalMinutes`(기본 10분)로 동기화 주기를,
`WeighSyncLookbackDays`(기본 30일)로 계근 데이터를 매번 다시 훑는 기간을 조정할 수 있습니다.
`MEASURE_RETR`이 계량일자(TDATE) 기준으로만 조회되고 수정일시 필터를 지원하지 않기 때문에,
검수/수정이 이 기간보다 늦게 들어오면 반영되지 않습니다 — 실제 업무에서 검수가 며칠까지
늦어질 수 있는지에 맞춰 조정하세요.

### 새 설정이 추가되면

`App.config.example`(템플릿, Git 추적됨)에 새 설정이 추가될 때가 있습니다. 그 항목만
로컬 `App.config`에 직접 옮겨 적으시면 됩니다 — `App.config` 자체는 추적 대상이 아니라서
자동으로 반영되지 않습니다.

### 기존에 `git update-index --skip-worktree`로 설정해두신 경우

더 이상 필요 없으니, 다음 명령으로 원래대로 되돌려두세요(안 해도 동작엔 지장 없지만,
찜찜하시면):

```
git update-index --no-skip-worktree src/ColumbusSync.BranchA/App.config
```

## 구현 상태

네 프로시저(`DP_SA015F00`, `DP_CM001F00`, `DP_CM009F00`, `DP_CM003F00`) 모두 **실제 본문을
받아서 컬럼명/필터 조건을 확인·반영했습니다.** 처음 버전에는 아래 버그들이 있었고 전부
수정되었습니다.

- `CV_Retr`/`CAR_RETR`을 `DP_SA015F00`으로 잘못 호출 — 해당 CMD 분기가 없는 프로시저라
  오류 없이 빈 결과만 돌아왔음. 실제로는 CV_Retr는 `DP_CM001F00`, CAR_RETR은 `DP_CM009F00`.
- `MEASURE_RETR`의 `@COPYN`/`@GUBUN`을 빈 값으로 보내고 있었음 — 이 프로시저는 `@COPYN`이
  `'A'`/`'Y'`/`'N'`, `@GUBUN`이 `'A'`/`'I'`/`'O'` 중 하나와 정확히 일치해야 WHERE절이 참이
  되는 구조라, 빈 값이면 무조건 0건. 지금은 `'A'`(전체)를 명시적으로 넘긴다.
- `FIRST_MEASURE_RETR`(1차 대기 목록)을 호출하지 않고 있었음 — `MEASURE_RETR`은
  `FWEIT<>0 AND SWEIT<>0`(완료 건)만 주기 때문에, 원본 화면처럼 대기 건까지 보려면 두
  CMD를 같이 호출해서 합쳐야 함. 지금은 둘 다 호출해서 합친다.
- 거래처(`DP_CM001F00`) 컬럼명이 짐작과 많이 달랐음 — 예: 대표자명은 `CEO`가 아니라
  `OWNAM`, 담당자명은 조인된 `PLNCD_NM`, 전화/팩스는 `TELNO`/`FAXNO`, 사업자번호는 `SANO`,
  종목은 `JONGK`, 주소는 이미 합쳐진 `ADDR`. `Source/MesSourceReader.cs`의 `GetCustomers()`
  주석에 실제 매핑을 정리해뒀다.
- 차량(`DP_CM009F00`)의 원본 테이블(`CAR_TEMPLATE`)은 순수 차량대장이 아니라 **"차량번호+
  거래처+품목 조합 템플릿"**이라, 운전자명/공차중량 컬럼이 아예 없다. 통합 허브 `VEHICLE`
  테이블에는 이 컬럼들이 있지만(B/C지점 mdb `TB_CAR`에는 실제로 있음), A지점에서는 항상
  `NULL`로 채워진다. `VehicleType`으로 매핑한 `CARML`이 정말 "차종"을 뜻하는지는 담당자
  확인이 필요하다.
- 품목(`DP_CM003F00`, CMD=`LIST_M`)도 `@STDYN`을 빈 값으로 보내면 무조건 0건이 나오는
  같은 함정이 있었음 — `'ALL'`을 명시적으로 넘기도록 구현했다. `ITEMAS` 원본에는 감량중량/
  감량율에 해당하는 컬럼이 없어서(B/C지점 mdb `TB_PUM`에는 `LossWt`/`LossPro`로 있음),
  통합 허브 `PRODUCT.LOSS_WEIGHT`/`LOSS_RATE`는 A지점에서는 항상 `NULL`이다. 단가는
  `BCOST`(매입원가)와 `OCOST`(매출원가) 두 개가 있는데, 계근 화면(`DP_SA015F00`)이 계근
  단가를 채울 때 `OCOST`를 쓰는 것으로 보여(차량 조회 쿼리에서 `I.OCOST AS UCOST`) 그것을
  `UNIT_PRICE`로 매핑했다.

입/출고 구분(`IN_OUT_TYPE`), 완료 여부(`IS_COMPLETED`)는 원본 값을 그대로 쓰지 않고
`SyncOrchestrator.Transform()`에서 통합 규칙으로 재계산합니다 (지점별 데이터 차이점 정리 문서 참고).

## 빌드

Visual Studio 2017+ 또는 `msbuild`로 `ColumbusSync.BranchA.csproj`를 빌드하면 됩니다.
DevExpress 등 외부 컴포넌트가 필요 없어 빌드 환경이 단순합니다.
