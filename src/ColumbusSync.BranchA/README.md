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

## 구현 상태 (스켈레톤)

- 거래처(`CV_Retr`), 차량(`CAR_RETR`), 계근 원장(`MEASURE_RETR`) 동기화 로직은 구현되어 있습니다.
- 품목(제품) 마스터는 `Source/MesSourceReader.cs`의 `GetProducts()`가 아직 비어 있습니다.
  MES의 `01CM/CM003F00`, `CM004F00` 화면이 실제로 호출하는 CMD 값을 확인한 뒤 채워야 합니다.
- 저장프로시저 반환 컬럼명은 `Columbus_total`의 `SA015F00.cs`, `CM001F00.cs`, `CM009F00.cs`
  코드에서 실제로 바인딩하는 컬럼명을 근거로 작성했습니다. 운영 반영 전 DBA/MES 담당자와
  한 번 더 대조 확인을 권장합니다.
- 입/출고 구분(`IN_OUT_TYPE`), 완료 여부(`IS_COMPLETED`)는 원본 값을 그대로 쓰지 않고
  `SyncOrchestrator.Transform()`에서 통합 규칙으로 재계산합니다 (지점별 데이터 차이점 정리 문서 참고).

## 빌드

Visual Studio 2017+ 또는 `msbuild`로 `ColumbusSync.BranchA.csproj`를 빌드하면 됩니다.
DevExpress 등 외부 컴포넌트가 필요 없어 빌드 환경이 단순합니다.
