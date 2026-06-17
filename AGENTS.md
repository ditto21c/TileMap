# Repository Guidelines

## 프로젝트 구조 및 모듈 구성
이 저장소는 Unity 6000.3 계열 2D 타일맵 프로토타입입니다. 런타임 코드는 `Assets/Scripts`에 있으며 영역별 네임스페이스 `TileMap.<Area>`를 사용합니다.

- `Assets/Scripts/Bootstrap`: 샘플 씬 구성, 런타임 타일 DB 생성, 카메라 연결
- `Assets/Scripts/Data`: `TileId`, `BiomeType`, `AutoTileSet`, `AutoTilePalette`, 타일 정의
- `Assets/Scripts/World`: 청크 기반 `WorldTileMap`, `MapChunk`, 레이어 데이터
- `Assets/Scripts/Generation`: 바이옴 생성, 청크 스트리밍, 경계 오버레이 규칙
- `Assets/Scripts/Rendering`: Ground/Overlay Tilemap 렌더링, 디버그 팔레트, 월드맵
- `Assets/Scripts/Movement`: 격자 이동, 탭 이동, A* 경로 탐색
- `Assets/Resources`: 오토타일 세트, Tile asset, 스프라이트 리소스
- `Docs`: 타일 세팅과 bitmask 설명 HTML

`Library`, `Temp`, `Logs`, `UserSettings`, `.vs`, `obj`, `*.csproj`, `*.sln`은 Unity/IDE 생성물로 취급합니다.

## 빌드, 테스트, 개발 명령
Unity 에디터 실행:

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.3.9f1\Editor\Unity.exe" -projectPath .
```

빠른 C# 컴파일 확인:

```powershell
dotnet build TileMap.sln
```

새 스크립트를 추가한 직후에는 Unity가 `.csproj`를 갱신하기 전까지 `dotnet build`가 실패할 수 있습니다. 이 경우 Unity 에디터에서 재컴파일을 먼저 확인합니다.

Edit Mode 테스트:

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.3.9f1\Editor\Unity.exe" -projectPath . -batchmode -quit -runTests -testPlatform EditMode -testResults Logs/EditModeResults.xml
```

## 코딩 스타일 및 명명 규칙
C#은 4칸 들여쓰기, 파일명과 대표 타입명 일치를 기본으로 합니다. 타입, 메서드, 프로퍼티, enum 값은 `PascalCase`, 지역 변수와 `[SerializeField] private` 필드는 `camelCase`를 사용합니다. 인스펙터 노출은 public 필드보다 `[SerializeField] private`을 우선합니다. 타일 값은 `TileId` enum에 추가하고, 한글 주석은 `Name = 1, // 설명.` 형식을 유지합니다.

## 테스트 가이드라인
현재 테스트 폴더는 없지만 결정적 로직은 Edit Mode 테스트 대상입니다. `WorldTileMap`, `BiomeGenerator`, `AStarPathfinder`, `AutoTileSet` 테스트는 `Assets/Tests/EditMode/*Tests.cs`에 둡니다. 입력, 씬 연결, 스트리밍 동작은 Play Mode 테스트로 분리합니다.

## 커밋 및 Pull Request 가이드라인
커밋 메시지는 짧은 명령형 한국어를 사용합니다. 예: `타일 레이어와 경계 오버레이 추가`. PR에는 변경 목적, 영향 영역, 테스트 결과, 시각 변경 스크린샷을 포함합니다. Unity `.asset`/`.meta`는 관련 에셋과 함께 커밋하되, 생성 폴더나 머신별 설정은 제외합니다.

## 에이전트 작업 지침
기존 사용자 변경을 되돌리지 않습니다. 코드 변경은 `Assets/Scripts` 중심으로 작게 유지하고, 타일 에셋 작업은 `Assets/Resources`와 `.meta`를 함께 다룹니다. 청크/렌더링 변경 후에는 `dotnet build TileMap.sln` 또는 Unity 콘솔로 컴파일 오류를 확인합니다.
