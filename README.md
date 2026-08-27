# ColumbusWeighing — 계량 조회/집계 프로그램 (A/B/C 지점 공용)

Visual Studio 2017 + DevExpress WinForms 기반의 **조회/집계 전용** 계량관리 프로그램입니다.
A/B/C 지점의 계근 데이터를 각 지점이 지금 쓰는 프로그램(A지점: MES, B/C지점: TS2020)에서
그대로 입력받고, 이 프로그램은 통합 허브 DB(`COLUMBUS_WEIGH_HUB`)에 모인 결과를
지점 구분 없이 조회/집계만 합니다. 실제 계량 등록(1차/2차/1회 계량 버튼)은 이 프로그램에
두지 않습니다 — 각 지점 원본 시스템은 전혀 변경하지 않는다는 전제입니다.

## 열기 전 준비 사항

1. Visual Studio 2017(15.9 이상)과 DevExpress WinForms(v18.1 기준으로 참조 작성)가
   설치되어 있어야 합니다.
2. `src/ColumbusWeighing/ColumbusWeighing.csproj` 의 `<Reference Include="DevExpress.*.v18.1" />`
   버전 접미사가 실제 설치된 DevExpress 버전과 다르면, 프로젝트를 열었을 때 참조가
   노란 경고 아이콘으로 표시됩니다. 이 경우 전체 참조의 `v18.1` 부분을 설치된 버전
   (예: `v19.2`, `v21.1` 등)으로 일괄 변경한 뒤 다시 참조를 추가해 주세요.
3. `ColumbusWeighing.sln` 을 Visual Studio 2017 로 열고 빌드/실행(F5)하면 됩니다.

## 프로젝트 구조

```
src/ColumbusWeighing/
  Program.cs                    진입점, DevExpress 스킨 초기화
  Forms/
    MainForm.cs/.Designer.cs    메인 화면(상단 로그/로그인 + 1차/2차 조회 그리드)
    LoginForm.cs/.Designer.cs   로그인 대화상자
  Controls/
    FirstWeighingControl        "1차 계량 대기" 조회 패널
    SecondWeighingControl       "2차계량 완료" 조회 패널 — 조회일자 기준
  Models/
    WeighingRecord, InOutType   계근 기록 도메인 모델
  Services/
    IWeighingRepository /
    InMemoryWeighingRepository  계근 기록 저장소(메모리, 데모 데이터 포함) — 통합 허브 DB 연동 시 교체
    AppLogService                상단 로그 패널에 표시되는 이벤트 로그
```

## 화면 구성과 동작

- **상단**: 통신/이벤트 로그(가운데), LOGIN 버튼과 로그인한 회사/사용자명(우측).
- **1차 계량 대기**: 아직 2차 계량이 완료되지 않은 건을 조회합니다.
  - `1차 전표`: 전표 출력은 자리만 마련해 두었습니다(`TODO: XtraReports 연결`).
- **2차계량 완료**: 조회일자를 기준으로 그 날 2차 계량까지 완료된 건을 조회합니다.
  1차중량/2차중량/순중량이 함께 표시됩니다.
  - `조회일자`: 날짜를 바꾸면 해당 일자의 완료 건만 다시 조회됩니다.
  - `2차 전표`: 마찬가지로 자리만 마련해 두었습니다.

계량 등록 버튼(1차계량/2차계량/1회계량)과 계근대 지시계 통신 서비스는 조회 전용
프로그램 방향에 맞춰 제거했습니다. 계량 입력은 각 지점 원본 프로그램에서 계속 이루어집니다.

## 실제 현장 적용 시 확장 포인트

- `InMemoryWeighingRepository` 를 통합 허브 DB(`COLUMBUS_WEIGH_HUB`, `sql/COLUMBUS_WEIGH_HUB_schema.sql`
  참고) 연동 리포지토리로 교체하세요. `IWeighingRepository` 인터페이스(`Records`, `Refresh(from, to)`)만
  구현하면 화면 코드는 수정할 필요가 없습니다.
- 거래처/차량/제품 마스터 조회 화면과 전표(XtraReports) 출력은 메뉴/버튼 자리만
  마련되어 있으며 별도 구현이 필요합니다.
- 원본 화면의 "계량 화면 설정" 팝업(표시 컬럼 선택)은 이번 작업 범위에서 제외했습니다.
  필요하시면 그리드 컬럼의 `Visible` 속성을 토글하는 설정 화면으로 추가하면 됩니다.
- A지점 데이터를 통합 허브 DB로 옮기는 동기화 잡은 `src/ColumbusSync.BranchA`를 참고하세요.
