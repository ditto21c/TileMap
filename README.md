# Unity 2D 로그라이트 월드 샘플 코드

이 샘플은 아래를 한 번에 포함합니다.

- `uint8` 기반 10000x10000 월드 데이터 구조
- 청크 기반 월드 로딩
- `BiomeGenerator` 기반 바이옴 자동 생성
- Unity `Tilemap` 렌더링
- 플레이어 타일 이동
- 터치/마우스 탭 이동
- A* 경로 탐색
- 카메라 추적
- 런타임 디버그 타일 팔레트

## 폴더

- `Assets/Scripts/Data`
- `Assets/Scripts/World`
- `Assets/Scripts/Generation`
- `Assets/Scripts/Rendering`
- `Assets/Scripts/Movement`
- `Assets/Scripts/Bootstrap`
- `Assets/Scripts/Utility`

## 빠른 사용법

1. 새 Unity 2D 프로젝트를 만든다.
2. 이 ZIP의 `Assets/Scripts` 폴더를 프로젝트 `Assets` 아래에 복사한다.
3. 빈 씬을 연다.
4. 빈 GameObject를 하나 만들고 이름을 `Bootstrap`으로 둔다.
5. `SampleSceneBootstrap` 컴포넌트를 붙인다.
6. 플레이하면 런타임에 아래가 자동 생성된다.
   - Grid
   - Ground Tilemap
   - WorldRoot
   - Player
   - Main Camera(없으면 생성)

## 조작

- 마우스 클릭 / 모바일 터치: 목표 칸까지 경로 탐색 후 이동
- `WASD` / 방향키: 한 칸 이동

## 핵심 클래스

### `WorldTileMap`
- 실제 월드 데이터 저장소
- `10000 x 10000` 크기 지원
- 내부적으로 청크(`64 x 64`) 단위 저장
- 타일 조회 / 수정 / 점유 처리 담당

### `BiomeGenerator`
- 좌표 기반으로 바이옴 결정
- 샘플 바이옴:
  - 초원
  - 폐허
  - 숲
  - 늪
  - 동굴/광산
  - 화산
  - 보스 코어
- 중앙 화산형 구조로 자동 생성

### `WorldStreamingController`
- 플레이어 주변 청크만 생성
- 플레이어가 청크를 넘으면 새로운 청크 생성
- 그때마다 타일맵 다시 렌더링

### `PlayerGridMover`
- 논리 좌표는 셀 단위
- 화면 표현은 부드러운 보간 이동
- 점유 예약 포함

### `TapToMoveController`
- 터치/클릭 위치를 셀 좌표로 변환
- A*로 경로 탐색
- 플레이어에게 경로 전달

### `RuntimeDebugTilePalette`
- 별도 타일 에셋 없이 색 타일을 런타임 생성
- 프로토타입 단계에서 바로 월드 확인 가능

## 교체 포인트

### 실제 아트 타일로 바꾸기
현재는 `RuntimeDebugTilePalette`가 색 타일을 생성합니다.
나중에는 이 부분을 아래 방식으로 바꾸면 됩니다.

- `TileId -> TileBase` 매핑 ScriptableObject 생성
- `WorldTilemapRenderer`에서 해당 `TileBase` 사용

### 적 / 자원 / 이벤트 시스템 연결
현재 샘플은 타일 ID만 생성합니다.
실제 게임에서는 아래 시스템을 추가로 붙이면 됩니다.

- `EnemySpawnResolver`
- `ResourceSpawner`
- `EventSpawner`
- `ChunkSaveLoader`

### UI 연결
SimplePixelUI 기준으로 붙일 때는 아래를 권장합니다.

- 현재 위치 바이옴 표시
- 현재 셀 좌표 표시
- 미니맵 청크 로딩 상태 표시
- 상호작용 버튼
- 자동 이동 취소 버튼

## 월드 구조 개요

샘플 월드는 대략 이런 형태입니다.

- 남쪽 외곽: 시작권 / 초원
- 남서: 폐허
- 서쪽: 숲
- 동쪽: 늪
- 북쪽: 동굴/광산
- 중앙: 화산
- 중심 핵: 보스 전장

## 주의

이 샘플은 **구조 검증용 프로토타입 코드**입니다.
실출시 단계에서는 아래를 추가하는 게 좋습니다.

- 저장/로드
- 청크 직렬화
- 동적 오브젝트 분리 저장
- 비동기 청크 생성
- 오브젝트 풀링
- 경로 탐색 캐시
- 전투 / AI / 전리품 시스템

