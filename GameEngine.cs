using System;
using System.Collections.Generic;
using System.Drawing;

namespace ResourceCollectorGame
{
    /// <summary>
    /// Главный движок игры.
    /// Управляет логикой игры, объектами и их взаимодействием.
    /// </summary>
    public class GameEngine
    {
        public Player Player { get; private set; }
        public Base PlayerBase { get; private set; }
        public List<ResourceNode> ResourceNodes { get; private set; }
        public List<CaveEntrance> CaveEntrances { get; private set; }
        public List<Monster> Monsters { get; private set; }
        public List<AutoBuilding> AutoBuildings { get; private set; }
        public bool IsInCave { get; private set; }
        public ResourceNode CurrentMiningNode { get; private set; }

        public int WoodCount { get; private set; }
        public int StoneCount { get; private set; }
        public int GoldCount { get; private set; }

        /// <summary>
        /// Базовая площадь, относительно которой считаем масштаб спавна.
        /// 1024x768 = 786432.
        /// </summary>
        private const float BaseArea = 800f * 600f;

        /// <summary>
        /// Коэффициент масштаба по площади экрана.
        /// </summary>
        private float AreaScale => (gameWidth * gameHeight) / BaseArea;

        /// <summary>
        /// Количество древних чертежей.
        /// </summary>
        public int AncientBlueprintsCount { get; private set; }

        private Random random;
        private float gameWidth;
        private float gameHeight;

        private List<ResourceNode> savedSurfaceNodes;
        private List<ResourceNode> savedCaveNodes;
        private List<Monster> savedMonsters;

        private bool isFirstInit;

        /// <summary>
        /// Аккумулятор времени для пассивной добычи.
        /// </summary>
        private float autoProductionTimer;

        /// <summary>
        /// Перезарядка удара игрока (в секундах).
        /// </summary>
        private const float AttackCooldown = 0.5f;

        /// <summary>
        /// Таймер с момента последнего удара.
        /// </summary>
        private float attackTimer;

        /// <summary>
        /// Флаг активной анимации удара.
        /// </summary>
        public bool IsAttacking { get; private set; }

        /// <summary>
        /// Длительность анимации удара (секунды).
        /// </summary>
        public const float AttackAnimationDuration = 0.25f;

        /// <summary>
        /// Нормализованный прогресс анимации удара (0..1).
        /// </summary>
        public float AttackProgress
        {
            get
            {
                if (!IsAttacking || AttackAnimationDuration <= 0f) return 0f;
                return 1f - (AttackAnimationTime / AttackAnimationDuration);
            }
        }


        /// <summary>
        /// Оставшееся время анимации удара.
        /// </summary>
        public float AttackAnimationTime { get; private set; }

        public GameEngine(float width, float height)
        {
            gameWidth = width;
            gameHeight = height;

            random = new Random();
            isFirstInit = true;

            savedSurfaceNodes = new List<ResourceNode>();
            savedCaveNodes = new List<ResourceNode>();
            savedMonsters = new List<Monster>();
            AutoBuildings = new List<AutoBuilding>();

            InitializeGame();
        }

        private void InitializeGame()
        {
            Player = new Player(gameWidth / 2 + 100, gameHeight / 2);
            PlayerBase = new Base(gameWidth / 2 - 40, gameHeight / 2 - 40);

            ResourceNodes = new List<ResourceNode>();
            CaveEntrances = new List<CaveEntrance>();
            Monsters = new List<Monster>();

            if (isFirstInit)
            {
                GenerateWorld();
                GenerateCaveContent();
                isFirstInit = false;
            }

            IsInCave = false;
            CurrentMiningNode = null;

            WoodCount = 0;
            StoneCount = 0;
            GoldCount = 0;
            AncientBlueprintsCount = 0;

            autoProductionTimer = 0f;
            attackTimer = AttackCooldown;
            IsAttacking = false;
            AttackAnimationTime = 0f;

            ResourceNodes = new List<ResourceNode>(savedSurfaceNodes);
        }

        private void GenerateWorld()
        {
            savedSurfaceNodes.Clear();

            // Считаем количество объектов с учётом площади
            int baseTrees = 3;
            int baseStones = 2;
            int baseCaves = 2;

            // Масштабируем и округляем минимум к 1
            int treeCount = Math.Max(1, (int)Math.Round(baseTrees * AreaScale));
            int stoneCount = Math.Max(1, (int)Math.Round(baseStones * AreaScale));
            int caveCount = Math.Max(1, (int)Math.Round(baseCaves * AreaScale));

            // Генерация деревьев
            for (int i = 0; i < treeCount; i++)
            {
                savedSurfaceNodes.Add(new ResourceNode(
                    ResourceType.Wood,
                    random.Next(100, (int)gameWidth - 100),
                    random.Next(100, (int)gameHeight - 100)
                ));
            }

            // Генерация камней
            for (int i = 0; i < stoneCount; i++)
            {
                savedSurfaceNodes.Add(new ResourceNode(
                    ResourceType.Stone,
                    random.Next(100, (int)gameWidth - 100),
                    random.Next(100, (int)gameHeight - 100)
                ));
            }

            // Генерация входов в пещеры
            CaveEntrances = new List<CaveEntrance>();
            for (int i = 0; i < caveCount; i++)
            {
                CaveEntrances.Add(new CaveEntrance(
                    random.Next(100, (int)gameWidth - 100),
                    random.Next(100, (int)gameHeight - 100)
                ));
            }

            // Копируем узлы в активный список
            ResourceNodes = new List<ResourceNode>(savedSurfaceNodes);

        }

        private void GenerateCaveContent()
        {
            savedCaveNodes.Clear();
            savedMonsters.Clear();

            // Базовые количества
            int baseGoldNodes = 3;
            int baseMonsters = 3;

            // Масштаб с учётом площади
            int goldCount = Math.Max(1, (int)Math.Round(baseGoldNodes * AreaScale));
            int monsterCount = Math.Max(1, (int)Math.Round(baseMonsters * AreaScale));

            // Генерация золотых узлов
            for (int i = 0; i < goldCount; i++)
            {
                savedCaveNodes.Add(new ResourceNode(
                    ResourceType.Gold,
                    random.Next(100, (int)gameWidth - 100),
                    random.Next(100, (int)gameHeight - 100)
                ));
            }

            // Генерация монстров
            for (int i = 0; i < monsterCount; i++)
            {
                Monster monster = new Monster(
                    random.Next(100, (int)gameWidth - 100),
                    random.Next(100, (int)gameHeight - 100)
                );
                monster.SetBounds(gameWidth, gameHeight);
                savedMonsters.Add(monster);
            }

        }

        public void Update(float deltaTime)
        {
            // Таймер атаки
            attackTimer += deltaTime;

            // Анимация удара
            if (IsAttacking)
            {
                AttackAnimationTime -= deltaTime;
                if (AttackAnimationTime <= 0f)
                {
                    IsAttacking = false;
                    AttackAnimationTime = 0f;
                }
            }

            if (IsInCave)
            {
                foreach (var monster in Monsters)
                {
                    monster.Update(Player.Position, deltaTime);
                }
            }

            if (CurrentMiningNode != null && CurrentMiningNode.IsBeingMined)
            {
                CurrentMiningNode.MiningProgress += deltaTime;
                if (CurrentMiningNode.MiningProgress >= CurrentMiningNode.MiningTime)
                {
                    CollectResource(CurrentMiningNode);
                    CurrentMiningNode.IsBeingMined = false;
                    CurrentMiningNode.MiningProgress = 0;
                    CurrentMiningNode = null;
                }
            }

            UpdateAutoProduction(deltaTime);

            // Обновление анимации игрока (ходьба)
            Player.UpdateAnimation(deltaTime);

        }

        private void UpdateAutoProduction(float deltaTime)
        {
            autoProductionTimer += deltaTime;
            if (autoProductionTimer < 1.0f)
                return;

            int ticks = (int)(autoProductionTimer / 1.0f);
            autoProductionTimer -= ticks * 1.0f;

            for (int i = 0; i < ticks; i++)
            {
                foreach (var building in AutoBuildings)
                {
                    float amount = building.GetProductionPerSecond();
                    int add = (int)Math.Round(amount);

                    switch (building.Type)
                    {
                        case AutoBuildingType.Sawmill:
                            WoodCount += add;
                            break;
                        case AutoBuildingType.Quarry:
                            StoneCount += add;
                            break;
                        case AutoBuildingType.GoldMine:
                            GoldCount += add;
                            break;
                    }
                }
            }
        }

        private void CollectResource(ResourceNode node)
        {
            switch (node.Type)
            {
                case ResourceType.Wood:
                    WoodCount++;
                    break;
                case ResourceType.Stone:
                    StoneCount++;
                    break;
                case ResourceType.Gold:
                    GoldCount++;
                    break;
            }
        }

        public bool CheckMonsterCollision()
        {
            foreach (var monster in Monsters)
            {
                // Если монстр слишком близко к игроку
                if (monster.GetDistance(Player.Position) < 30f)
                {
                    // Монстр крадет ресурсы не чаще 1 раза в секунду
                    if (monster.CanSteal())
                    {
                        // Крадём 15% каждого ресурса (дерево, камень, золото), чертежи не трогаем
                        int stolenWood = (int)(WoodCount * 0.15f);
                        int stolenStone = (int)(StoneCount * 0.15f);
                        int stolenGold = (int)(GoldCount * 0.15f);

                        WoodCount = Math.Max(0, WoodCount - stolenWood);
                        StoneCount = Math.Max(0, StoneCount - stolenStone);
                        GoldCount = Math.Max(0, GoldCount - stolenGold);

                        return stolenWood > 0 || stolenStone > 0 || stolenGold > 0;
                    }
                }
            }

            return false;
        }


        public bool StartMining()
        {
            if (CurrentMiningNode != null) return false;

            foreach (var node in ResourceNodes)
            {
                if (Player.Bounds.IntersectsWith(node.Bounds))
                {
                    CurrentMiningNode = node;
                    node.IsBeingMined = true;
                    return true;
                }
            }

            return false;
        }

        public void StopMining()
        {
            if (CurrentMiningNode != null)
            {
                CurrentMiningNode.IsBeingMined = false;
                CurrentMiningNode.MiningProgress = 0;
                CurrentMiningNode = null;
            }
        }

        public bool TryEnterCave()
        {
            if (IsInCave) return false;

            foreach (var cave in CaveEntrances)
            {
                if (Player.Bounds.IntersectsWith(cave.Bounds))
                {
                    IsInCave = true;

                    ResourceNodes = savedCaveNodes;
                    Monsters      = savedMonsters;

                    Player.Position = new PointF(100, gameHeight / 2);

                    return true;
                }
            }

            return false;
        }

        public void ExitCave()
        {
            IsInCave = false;
            ResourceNodes = savedSurfaceNodes;
            Monsters = new List<Monster>();
            Player.Position = new PointF(gameWidth / 2 + 100, gameHeight / 2);

        }

        public bool TryUpgradeBase()
        {
            if (!PlayerBase.CanUpgrade())
                return false;

            int reqWood = PlayerBase.GetRequiredWood();
            int reqStone = PlayerBase.GetRequiredStone();
            int reqGold = PlayerBase.GetRequiredGold();
            int reqBlueprints = PlayerBase.GetRequiredBlueprints();

            if (WoodCount >= reqWood &&
                StoneCount >= reqStone &&
                GoldCount >= reqGold &&
                AncientBlueprintsCount >= reqBlueprints)
            {
                WoodCount -= reqWood;
                StoneCount -= reqStone;
                GoldCount -= reqGold;
                AncientBlueprintsCount -= reqBlueprints;

                PlayerBase.Upgrade();
                return true;
            }

            return false;
        }

        public void MovePlayer(float deltaX, float deltaY)
        {
            Player.MoveX(deltaX, gameWidth);
            Player.MoveY(deltaY, gameHeight);
        }

        /// <summary>
        /// Попытка удара по ближайшему монстру.
        /// </summary>
        public bool TryAttackMonster()
        {
            if (!IsInCave) return false;
            if (attackTimer < AttackCooldown) return false;
            // Хитбокс удара
            RectangleF hitbox = GetAttackHitbox();

            // Запускаем анимацию удара
            IsAttacking = true;
            AttackAnimationTime = AttackAnimationDuration;
            attackTimer = 0f;

            // Ищем любого монстра, чьи границы пересекаются с хитбоксом
            Monster target = null;
            foreach (var monster in Monsters)
            {
                if (hitbox.IntersectsWith(monster.Bounds))
                {
                    target = monster;
                    break;
                }
            }

            if (target == null)
            {
                // Никого не задели, но удар всё равно произошёл
                return true;
            }

            // Наносим урон
            bool died = target.TakeHit();
            AudioManager.PlayHit();
            if (died)
            {
                AncientBlueprintsCount++;
                savedMonsters.Remove(target);

                // Спавним нового монстра
                Monster newMonster = new Monster(
                    random.Next(100, (int)gameWidth - 100),
                    random.Next(100, (int)gameHeight - 100)
                );
                newMonster.SetBounds(gameWidth, gameHeight);
                savedMonsters.Add(newMonster);
            }

            return true;


            
        }
        /// <summary>
        /// Расчёт хитбокса удара вокруг игрока.
        /// Используется и для логики, и для отрисовки.
        /// </summary>
        public RectangleF GetAttackHitbox()
        {
            RectangleF pb = Player.Bounds;

            // Размер удара: можно менять под вкус
            float attackSize = Math.Max(pb.Width, pb.Height) * 4.0f;

            float x = pb.X + pb.Width / 2 - attackSize / 2;
            float y = pb.Y + pb.Height / 2 - attackSize / 2;

            return new RectangleF(x, y, attackSize, attackSize);
        }

        public bool TryBuildAutoBuilding(AutoBuildingType type)
        {
            foreach (var b in AutoBuildings)
            {
                if (b.Type == type)
                    return false;
            }

            AutoBuilding temp = new AutoBuilding(type, 0, 0);
            var cost = temp.GetBuildCost();
            int bpCost = temp.GetBuildBlueprintCost();

            if (WoodCount < cost.wood ||
                StoneCount < cost.stone ||
                GoldCount < cost.gold ||
                AncientBlueprintsCount < bpCost)
                return false;

            WoodCount -= cost.wood;
            StoneCount -= cost.stone;
            GoldCount -= cost.gold;
            AncientBlueprintsCount -= bpCost;

            float offsetX = 0;
            float offsetY = 0;

            switch (type)
            {
                case AutoBuildingType.Sawmill:
                    offsetX = -200;  
                    offsetY = -60;   
                    break;
                case AutoBuildingType.Quarry:
                    offsetX = 200;   
                    offsetY = -60;   
                    break;
                case AutoBuildingType.GoldMine:
                    offsetX = 0;
                    offsetY = 200;   
                    break;
            }


            float x = PlayerBase.Position.X + offsetX;
            float y = PlayerBase.Position.Y + offsetY;

            AutoBuilding building = new AutoBuilding(type, x, y);
            AutoBuildings.Add(building);
            AudioManager.PlayBuild();

            return true;
        }

        public bool TryUpgradeAutoBuilding(AutoBuildingType type)
        {
            AutoBuilding target = AutoBuildings.Find(b => b.Type == type);
            if (target == null)
                return false;

            if (!target.CanUpgrade())
                return false;

            var cost = target.GetUpgradeCost();
            int bpCost = target.GetUpgradeBlueprintCost();

            if (WoodCount < cost.wood ||
                StoneCount < cost.stone ||
                GoldCount < cost.gold ||
                AncientBlueprintsCount < bpCost)
                return false;

            WoodCount -= cost.wood;
            StoneCount -= cost.stone;
            GoldCount -= cost.gold;
            AncientBlueprintsCount -= bpCost;

            target.Upgrade();
            AudioManager.PlayBuild();
            return true;
        }
    }
}
