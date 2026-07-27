# ColumbusWeighing — 무인계근관리 프로그램

Visual Studio 2017 + DevExpress WinForms 기반의 무인계근관리(계량대) 프로그램 뼈대입니다.
참고 화면(계량관리 프로그램 TS2020)의 팝업(계량 화면 설정)을 제외한 뒷화면 — 상단 중량
표시/로그/로그인 영역, **1차 계량** 목록, 그리고 팝업에 가려져 있던 **2차계량** 목록 —
을 새로 작성했습니다.

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
    MainForm.cs/.Designer.cs    메인 화면(상단 계량 표시/로그/로그인 + 1차/2차 그리드)
    LoginForm.cs/.Designer.cs   로그인 대화상자
  Controls/
    WeightDisplayControl        중량 LED 스타일 표시기(안정/변동 색상 전환)
    FirstWeighingControl        "1차 계량" 패널 — 2차 계량 대기 목록
    SecondWeighingControl       "2차계량" 패널 — 조회일자 기준 완료 목록 (요청하신 화면)
  Models/
    WeighingRecord, InOutType   계근 기록 도메인 모델
  Services/
    IScaleIndicatorService /
    SimulatedScaleIndicatorService   계근대 지시계 통신 추상화 + 모의(시뮬레이션) 구현체
    IWeighingRepository /
    InMemoryWeighingRepository       계근 기록 저장소(메모리, 데모 데이터 포함) — DB 연동 시 교체
    AppLogService                    상단 로그 패널에 표시되는 이벤트 로그
```

## 화면 구성과 동작

- **상단**: 좌측에 현재 중량을 큼직하게 표시(정지 시 녹색, 변동 시 호박색), 가운데는
  통신/이벤트 로그, 우측은 LOGIN 버튼과 로그인한 회사/사용자명.
- **1차 계량**: 아직 2차 계량이 완료되지 않은 건만 표시됩니다.
  - `1차계량(F5)`: 차량번호/거래처/제품을 입력받아 현재 중량을 1차중량으로 등록합니다.
  - `2차계량(F6)`: 목록에서 선택한 건에 대해 현재 중량을 2차중량으로 확정하고,
    해당 건은 자동으로 2차계량 목록으로 이동합니다.
  - `1차 전표`: 전표 출력은 자리만 마련해 두었습니다(`TODO: XtraReports 연결`).
- **2차계량** (원본 화면에서 팝업에 가려져 있던 영역): 조회일자를 기준으로 그 날
  2차 계량까지 완료된 건을 표시합니다. 1차중량/2차중량/순중량이 함께 표시됩니다.
  - `조회일자`: 날짜를 바꾸면 해당 일자의 완료 건만 다시 조회됩니다.
  - `1회계량(F7)`: 등록된 공차중량을 입력받아 1차/2차를 한 번에 확정합니다(사전 등록된
    차량 공차를 사용하는 1회 계량 방식).
  - `2차 전표`: 마찬가지로 자리만 마련해 두었습니다.

## 실제 현장 적용 시 확장 포인트

- `SimulatedScaleIndicatorService` 를 실제 지시계와 통신하는 `SerialScaleIndicatorService`
  (RS-232/RS-485, `System.IO.Ports.SerialPort`)로 교체하세요. `App.config` 의
  `ScalePortName`/`ScaleBaudRate`/`UseSimulatedScale` 값을 사용하도록 `Program.cs` 에서
  분기하면 됩니다.
- `InMemoryWeighingRepository` 를 실제 DB(MSSQL 등) 연동 리포지토리로 교체하세요.
  `IWeighingRepository` 인터페이스만 구현하면 화면 코드는 수정할 필요가 없습니다.
- 거래처/차량/제품 마스터 관리 화면과 전표(XtraReports) 출력은 메뉴/버튼 자리만
  마련되어 있으며 별도 구현이 필요합니다.
- 원본 화면의 "계량 화면 설정" 팝업(표시 컬럼 선택)은 이번 작업 범위에서 제외했습니다.
  필요하시면 그리드 컬럼의 `Visible` 속성을 토글하는 설정 화면으로 추가하면 됩니다.
