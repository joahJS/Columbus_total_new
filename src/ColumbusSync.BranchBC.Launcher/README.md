# ColumbusSync.BranchBC.Launcher

`ColumbusSync.BranchBC`(B/C지점 mdb 동기화 워커)를 실행하기 전에, **그 PC에 설치된 ACE
OLEDB(Access 파일을 읽는 드라이버)가 32비트인지 64비트인지 자동으로 판단해서 맞는 쪽을
대신 실행해주는 작은 런처**입니다.

## 왜 필요한가요

mdb(Access) 파일은 32비트 드라이버로만, 또는 64비트 드라이버로만 읽을 수 있고 — 이건
프로세스 하나가 둘 다 동시에 쓸 수 없는 Windows 자체의 제약입니다. PC마다 어느 쪽
드라이버가 깔려있는지 다를 수 있어서, 설치하는 사람이 매번 비트수를 확인해서 맞는
빌드를 골라야 하는 번거로움이 있었습니다. 이 런처가 그 판단을 대신합니다.

## 배포 폴더 구성

```
ColumbusSync.BranchBC\              (B/C지점 PC에 통째로 복사)
  ColumbusSync.BranchBC.Launcher.exe   <- 이걸 실행
  x86\
    ColumbusSync.BranchBC.exe
    ColumbusSync.BranchBC.exe.config     (= App.config 내용)
  x64\
    ColumbusSync.BranchBC.exe
    ColumbusSync.BranchBC.exe.config
```

런처는 실행되면:
1. 레지스트리에서 `Microsoft.ACE.OLEDB.12.0`이 64비트로 등록되어 있는지, 32비트로
   등록되어 있는지 확인합니다 (둘 다 있으면 64비트를 우선 선택).
2. 그에 맞는 `x86\` 또는 `x64\` 폴더의 `ColumbusSync.BranchBC.exe`를 실행하고, 자신은
   종료합니다. 이후 실제 동기화는 그 워커 프로세스가 계속 담당합니다(로그 창이 그대로
   남아있는 게 정상입니다).
3. 둘 다 없으면 "Access Database Engine을 설치하세요"라는 안내를 출력하고 아무것도
   실행하지 않습니다.

## 배포 패키지 만들기 (개발 PC에서)

1. `src/ColumbusSync.BranchBC/App.config.example`을 복사해서 같은 폴더에 `App.config`로
   만들고, 실제 값(허브 DB 접속정보, `BranchCode`, `MdbFilePath`)을 채웁니다. `MdbFilePath`는
   **실제로 배포할 그 PC** 기준 경로여야 하니, 지점마다 이 값을 다르게 해서 각각 따로
   빌드/패키징해야 합니다(B지점용 패키지, C지점용 패키지 두 벌).
2. 이 폴더의 `build-package.bat`을 **탐색기에서 그냥 더블클릭**합니다. ("Developer
   Command Prompt"를 따로 열 필요 없습니다 — Visual Studio가 항상 같이 설치해두는
   `vswhere.exe`로 스스로 msbuild 위치를 찾습니다.)
3. 저장소 루트의 `dist\ColumbusSync.BranchBC\` 폴더가 만들어집니다 — 이 폴더를 통째로
   해당 지점 PC에 복사하고 `ColumbusSync.BranchBC.Launcher.exe`를 실행하면 됩니다.

수동으로 하고 싶다면(스크립트 없이), `ColumbusSync.BranchBC.csproj`를
`msbuild ... /p:Platform=x86`과 `/p:Platform=x64`로 각각 빌드하고, 이 프로젝트도
빌드해서 위 폴더 구조에 맞게 직접 복사해도 됩니다.

## 참고

- 이 런처 자신은 AnyCPU라 비트수 제약이 없습니다 — Access 파일을 직접 다루지 않고,
  그냥 알맞은 워커를 실행만 해주기 때문입니다.
- 워커(`ColumbusSync.BranchBC`)의 `App.config` 설정 방법, 필수 사전 설치(Access Database
  Engine) 등 나머지 내용은 `../ColumbusSync.BranchBC/README.md`를 참고하세요.
