# 저장소 가이드라인

## 프로젝트 구조 및 모듈 구성
이 저장소는 Unity 6000.3.9f1 기반 2D 타일맵 프로토타입입니다. 런타임 코드는 `Assets/Scripts`에 있으며 역할별로 나뉩니다.

- `Bootstrap`: 샘플 씬 구성, 카메라 추적, 런타임 데이터베이스 생성
- `Data`: 타일 ID, 바이옴 타입, 타일 정의, 타일 데이터베이스
- `World`: 청크 기반 `WorldTileMap` 저장소와 `MapChunk` 데이터
- `Generation`: 바이옴 생성 및 플레이어 주변 월드 스트리밍
- `Rendering`: 타일맵 렌더링과 디버그 타일 팔레트 생성
- `Movement`: 플레이어 격자 이동, 탭 이동, A* 경로 탐색
- `Utility`: 스프라이트 및 노이즈 유틸리티

씬은 `Assets/Scenes`, 렌더 파이프라인과 2D 설정은 `Assets/Settings`에 있습니다. 패키지 상태는 `Packages`, Unity 프로젝트 설정은 `ProjectSettings`에서 관리합니다. `Library`, `Temp`, `Logs`, `UserSettings`는 로컬 생성 데이터로 취급합니다.

## 빌드, 테스트, 개발 명령
고정된 Unity 에디터 버전으로 프로젝트를 엽니다.

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.3.9f1\Editor\Unity.exe" -projectPath .
```

Edit Mode 테스트를 배치 모드로 실행합니다.

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.3.9f1\Editor\Unity.exe" -projectPath . -batchmode -quit -runTests -testPlatform EditMode -testResults Logs/EditModeResults.xml
```

Play Mode 테스트는 `-testPlatform PlayMode`로 바꿔 실행합니다. 현재 별도 빌드 스크립트는 없으므로 Unity Build Profiles 또는 Build Settings를 사용하세요. CI 배치 빌드가 필요하면 먼저 `Editor` 빌드 메서드를 추가합니다.

## 코딩 스타일 및 명명 규칙
C# 코드는 4칸 들여쓰기를 사용하고, 파일명은 대표 타입명과 일치시킵니다. 예: `WorldTileMap.cs`. 네임스페이스는 `TileMap.<Area>` 형식을 유지합니다. 타입, 메서드, 프로퍼티, enum 값은 `PascalCase`, 매개변수, 지역 변수, private 직렬화 필드는 `camelCase`를 사용합니다. Unity 인스펙터 값은 public 필드보다 `[SerializeField] private` 필드를 우선합니다.

## 테스트 가이드라인
Unity Test Framework 패키지는 설치되어 있지만 아직 테스트 폴더는 없습니다. 결정적 로직인 `BiomeGenerator`, `WorldTileMap`, `AStarPathfinder`는 `Assets/Tests/EditMode`에 Edit Mode 테스트를 추가하세요. 씬, 이동, 스트리밍 동작은 `Assets/Tests/PlayMode`에 Play Mode 테스트를 둡니다. 테스트 파일명은 대상 시스템을 기준으로 `WorldTileMapTests.cs`처럼 작성합니다.

## 커밋 및 Pull Request 가이드라인
현재 체크아웃에는 Git 히스토리가 없어 기존 커밋 규칙을 추론할 수 없습니다. 커밋 메시지는 `Add chunk streaming tests`, `Fix occupied cell clearing`처럼 짧은 명령형으로 작성하세요. Pull Request에는 게임플레이 영향, 변경된 영역, 관련 이슈를 적고, 씬이나 렌더링 변경이 보이면 스크린샷 또는 짧은 영상을 포함합니다.

## 에이전트 작업 지침
변경 범위는 `Assets`, `Packages`, `ProjectSettings`에 집중하세요. 로컬 에디터 상태를 진단하는 경우가 아니라면 Unity 생성 폴더를 수정하지 말고, 머신별 설정 파일은 커밋하지 않습니다.
