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

`connectionStrings` 두 개를 실제 값으로 채워야 합니다.

| 이름 | 용도 | 현재 알려진 서버 정보 |
|---|---|---|
| `BranchA_Mes` | A지점 MES 원본 DB | `sql16ssd-006.localnet.kr,1433` / DB `columbusdb_pineit` |
| `IntegrationHub` | 통합 허브 DB | `121.66.17.30,16433` / DB `COLUMBUS_WEIGH_HUB` |

계정/비밀번호는 소스에 커밋하지 말고 배포 PC의 `App.config`에서만 채워 넣으세요.

`appSettings`의 `SyncIntervalMinutes`(기본 10분)로 동기화 주기를 조정할 수 있습니다.

### 비밀번호 보호 (중요)

`App.config`는 Git 추적 대상 파일입니다. 로컬에서 실제 비밀번호를 채워 넣은 뒤 실수로
`git add -A`/`git commit -a` 등으로 같이 커밋해버리면, 나중에 값을 지워도 **커밋 이력에는
비밀번호가 그대로 남습니다.** (실제로 한 번 이런 일이 있었고, 그때 노출된 비밀번호는
즉시 변경했습니다.)

이 저장소를 받아서 실제 값을 채워 넣을 PC에서는, 아래 명령을 한 번만 실행해두세요.
이후로는 이 파일을 로컬에서 수정해도 `git status`/`git commit`에 걸리지 않습니다.

```
git update-index --skip-worktree src/ColumbusSync.BranchA/App.config
```

되돌리려면(파일을 다시 git 추적 대상으로): `git update-index --no-skip-worktree src/ColumbusSync.BranchA/App.config`

## 구현 상태

- **계근 원장(`MEASURE_RETR`/`FIRST_MEASURE_RETR`)은 `DP_SA015F00` 프로시저 실제 본문을
  확인하고 그에 맞춰 검증된 상태**입니다. 처음 버전은 `CV_Retr`/`CAR_RETR`까지 전부
  `DP_SA015F00`으로 잘못 호출하고 있었는데(해당 CMD 분기가 없어 오류 없이 빈 결과만 돌아옴),
  실제 화면 소스(`SA015F00.cs`, `CM001F00.cs`, `CM009F00.cs`)의 `PROCEDURE_ID` 상수를
  다시 확인해서 CV_Retr는 `DP_CM001F00`, CAR_RETR은 `DP_CM009F00`으로 바로잡았습니다.
  또한 `MEASURE_RETR`은 `@COPYN`/`@GUBUN`이 `'A'`/`'Y'`/`'N'`, `'A'`/`'I'`/`'O'` 중 하나가
  아니면 WHERE절 전체가 거짓이 되어 무조건 0건이 나오는 구조라, 빈 값 대신 `'A'`를 명시적으로
  넘기도록 고쳤습니다. 화면과 동일하게 `FIRST_MEASURE_RETR`(1차 대기)도 같이 호출해 합칩니다.
- **거래처(`DP_CM001F00`)/차량(`DP_CM009F00`) 반환 컬럼명은 아직 실제 프로시저 본문으로
  검증하지 못했습니다.** 화면 코드에서 저장 시 쓰는 파라미터명(거래처: CVCOD/CVNAM/CEO/
  DAMDANG/TEL/FAX/BUSSNO/UPTAE/JONGMOK/RK 추정, 차량: SEQNO/CARML/CARNO/CVCOD/ITCOD/RK만
  확인됨 — 차종/운전자/공차중량 컬럼은 이 MES 화면에 아예 없을 가능성이 있습니다)만 근거로
  추정한 상태라, `DP_SA015F00`처럼 실제 본문을 확인하면 한 번 더 맞춰야 합니다.
- 품목(제품) 마스터는 `Source/MesSourceReader.cs`의 `GetProducts()`가 아직 비어 있습니다.
  MES의 `01CM/CM003F00`, `CM004F00` 화면이 실제로 호출하는 `PROCEDURE_ID`/CMD 값을 확인한
  뒤 채워야 합니다.
- 입/출고 구분(`IN_OUT_TYPE`), 완료 여부(`IS_COMPLETED`)는 원본 값을 그대로 쓰지 않고
  `SyncOrchestrator.Transform()`에서 통합 규칙으로 재계산합니다 (지점별 데이터 차이점 정리 문서 참고).

## 빌드

Visual Studio 2017+ 또는 `msbuild`로 `ColumbusSync.BranchA.csproj`를 빌드하면 됩니다.
DevExpress 등 외부 컴포넌트가 필요 없어 빌드 환경이 단순합니다.
