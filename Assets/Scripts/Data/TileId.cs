using System;

namespace TileMap.Data
{
    public enum TileId : byte
    {
        // 빈 타일 / 생성되지 않은 영역
        Void = 0, // 빈 타일 / 생성되지 않은 영역.

        // 시작 지점 / 이벤트 / 보상
        SpawnPlayer = 2, // 플레이어 시작 위치.
        EventMinor = 8, // 소형 이벤트 발생 지점.
        TreasureMinor = 10, // 소형 보물 지점.

        // 초원 / 폐허 / 기본 필드
        GrassA = 16, // 기본 초원 바닥.
        GrassB = 17, // 변형 초원 바닥.
        DirtA = 19, // 흙 바닥.
        RoadA = 21, // 도로 / 길.
        TallGrass = 24, // 키 큰 풀.
        RockLarge = 28, // 큰 바위. 기본 설정에서는 이동 불가.
        RuinFloor = 32, // 폐허 바닥.
        OldWall = 35, // 오래된 벽. 기본 설정에서는 이동 및 시야 차단.
        Campfire = 39, // 모닥불 지점.
        MerchantSpot = 40, // 상인 위치.
        EnemyTier1 = 44, // 초반 적 위치.
        OreCopper = 48, // 구리 광석.
        WaterShallow = 54, // 얕은 물. 기본 설정에서는 이동 가능하지만 느림.
        WaterDeep = 55, // 깊은 물. 기본 설정에서는 이동 불가.

        // 숲 / 늪
        ForestFloorA = 64, // 숲 바닥 기본형.
        ForestFloorB = 65, // 숲 바닥 변형.
        BushDense = 69, // 빽빽한 덤불. 기본 설정에서는 이동 및 시야 차단.
        TreePine = 71, // 소나무. 기본 설정에서는 이동 및 시야 차단.
        SwampShallow = 75, // 얕은 늪지.
        SwampDeep = 76, // 깊은 늪지. 기본 설정에서는 이동 불가.
        EnemyTier3 = 80, // 중급 적 위치.
        ResourceWood = 83, // 목재 자원.
        OreIron = 86, // 철 광석.
        ShrineNature = 91, // 자연 제단.
        TreasureForest = 99, // 숲 보물 지점.

        // 동굴 / 광산
        CaveFloorA = 112, // 동굴 바닥.
        Pit = 118, // 구덩이. 기본 설정에서는 이동 불가.
        MineTrack = 120, // 광산 레일.
        OreSilver = 122, // 은 광석.
        EnemyCave1 = 129, // 동굴 적 위치.
        CaveBossGate = 132, // 동굴 보스 관문.
        ForgeUnderground = 137, // 지하 대장간.
        TreasureCave = 148, // 동굴 보물 지점.

        // 화산 / 보스 코어
        AshGround = 160, // 화산 재 바닥.
        LavaShallow = 163, // 얕은 용암. 기본 설정에서는 이동 가능하지만 피해가 있음.
        LavaDeep = 164, // 깊은 용암. 기본 설정에서는 이동 불가.
        EnemyFire1 = 173, // 화염 적 위치.
        BossArena = 178, // 보스 전장.
        ShrineFire = 181, // 불의 제단.
        OreMythril = 183, // 미스릴 광석.
        EndPortal = 200, // 엔딩 포탈.
    }
}
