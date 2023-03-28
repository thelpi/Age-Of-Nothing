namespace Age_Of_Nothing_Unit_Tests.ControllerTests
{
    public class SetTargetPositionsOnFocusedTests
    {
        /*[Fact]
        public void SetTargetPositionsOnFocused_WhenSourceAreVillagers_WhenTargetIsMine_WhenMarketExists_CreateCycleBetweenMineCenterAndMarketCenter()
        {
            var controller = new Controller(true);
            controller.Initialize();

            var pointFocusAll = new Rect(0, 0, 1000, 1000);
            var pointOnMineButNotCenter = new Point(399, 121);

            controller.FocusOnZone(pointFocusAll);
            controller.SetTargetPositionsOnFocused(pointOnMineButNotCenter);

            var sprites = controller
                .GetPrivateMemberValue<ObservableCollection<Sprite>>("_sprites");

            var singleMine = sprites.OfType<RockMine>().First();
            var singleMarket = sprites.OfType<Market>().First();

            var villagers = sprites.OfType<Villager>();
            foreach (var villager in villagers)
            {
                Assert.True(villager.Focused);

                var pathCycle = villager
                    .GetPrivateMemberValue<LinkedList<(Point point, Sprite target)>, Unit>("_pathCycle");
                var currentPathTarget = villager
                    .GetPrivateMemberValue<LinkedListNode<(Point point, Sprite target)>, Unit>("_currentPathTarget");
                var isPathLoop = villager
                    .GetPrivateMemberValue<bool, Unit>("_isPathLoop");

                Assert.True(isPathLoop);
                Assert.Equal(singleMine, currentPathTarget.Value.target);
                Assert.Equal(singleMine.Center, currentPathTarget.Value.point);

                Assert.Equal(2, pathCycle.Count);
                var current = pathCycle.First;
                Assert.Equal(singleMine, current.Value.target);
                Assert.Equal(singleMine.Center, current.Value.point);

                current = current.Next;
                Assert.Equal(singleMarket, current.Value.target);
                Assert.Equal(singleMarket.Center, current.Value.point);
            }
        }*/

        /*[Fact]
        public void SetTargetPositionsOnFocused_WhenSourceAreVillagers_WhenTargetIsForest_WhenMarketExists_CreateCycleBetweenClosestForestCenterAndMarketCenter()
        {
            var controller = new Controller(true);
            controller.Initialize();

            var pointFocusAll = new Rect(0, 0, 1000, 1000);
            var pointOnNotCloseForest = new Point(990, 290);

            controller.FocusOnZone(pointFocusAll);
            controller.SetTargetPositionsOnFocused(pointOnNotCloseForest);

            var sprites = controller
                .GetPrivateMemberValue<ObservableCollection<Sprite>>("_sprites");

            var singleMarket = sprites.OfType<Market>().First();

            var forests = sprites.OfType<Forest>();

            var topLeftForest = forests
                .OrderBy(x => x.Center.X)
                .ThenBy(x => x.Center.Y)
                .First();
            var bottomLeftForest = forests
                .OrderBy(x => x.Center.X)
                .ThenByDescending(x => x.Center.Y)
                .First();

            var villagers = sprites.OfType<Villager>();
            var bottomRightVillager = villagers
                .OrderByDescending(x => x.Center.X)
                .First();

            foreach (var villager in villagers)
            {
                Assert.True(villager.Focused);

                var pathCycle = villager
                    .GetPrivateMemberValue<LinkedList<(Point point, Sprite target)>, Unit>("_pathCycle");
                var currentPathTarget = villager
                    .GetPrivateMemberValue<LinkedListNode<(Point point, Sprite target)>, Unit>("_currentPathTarget");
                var isPathLoop = villager
                    .GetPrivateMemberValue<bool, Unit>("_isPathLoop");

                var closestForest = bottomRightVillager == villager
                    ? bottomLeftForest
                    : topLeftForest;

                Assert.True(isPathLoop);
                Assert.Equal(closestForest, currentPathTarget.Value.target);
                Assert.Equal(closestForest.Center, currentPathTarget.Value.point);

                Assert.Equal(2, pathCycle.Count);
                var current = pathCycle.First;
                Assert.Equal(closestForest, current.Value.target);
                Assert.Equal(closestForest.Center, current.Value.point);

                current = current.Next;
                Assert.Equal(singleMarket, current.Value.target);
                Assert.Equal(singleMarket.Center, current.Value.point);
            }
        }*/

        /*[Fact]
        public void SetTargetPositionsOnFocused_WhenSourceAreVillagers_WhenTargetIsMarket_CreatePathToMarketCenter()
        {
            var controller = new Controller(true);
            controller.Initialize();

            var pointFocusAll = new Rect(0, 0, 1000, 1000);
            var pointOnMarketButNotCenter = new Point(601, 501);

            controller.FocusOnZone(pointFocusAll);
            controller.SetTargetPositionsOnFocused(pointOnMarketButNotCenter);

            var sprites = controller
                .GetPrivateMemberValue<ObservableCollection<Sprite>>("_sprites");

            var singleMarket = sprites.OfType<Market>().First();

            var villagers = sprites.OfType<Villager>();
            foreach (var villager in villagers)
            {
                Assert.True(villager.Focused);

                var pathCycle = villager
                    .GetPrivateMemberValue<LinkedList<(Point point, Sprite target)>, Unit>("_pathCycle");
                var currentPathTarget = villager
                    .GetPrivateMemberValue<LinkedListNode<(Point point, Sprite target)>, Unit>("_currentPathTarget");
                var isPathLoop = villager
                    .GetPrivateMemberValue<bool, Unit>("_isPathLoop");

                Assert.False(isPathLoop);
                Assert.Equal(singleMarket, currentPathTarget.Value.target);
                Assert.Equal(singleMarket.Center, currentPathTarget.Value.point);

                Assert.Single(pathCycle);
                var current = pathCycle.First;
                Assert.Equal(singleMarket, current.Value.target);
                Assert.Equal(singleMarket.Center, current.Value.point);
            }
        }*/

        /*[Fact]
        public void SetTargetPositionsOnFocused_WhenSourceIsAnyUnit_WhenTargetIsBlankPoint_CreateDirectPathToPoint()
        {
            var controller = new Controller(true);
            controller.Initialize();

            var pointFocusAll = new Rect(0, 0, 1000, 1000);
            var blankPoint = new Point(5, 5);

            controller.FocusOnZone(pointFocusAll);
            controller.SetTargetPositionsOnFocused(blankPoint);

            var sprites = controller
                .GetPrivateMemberValue<ObservableCollection<Sprite>>("_sprites");

            var villagers = sprites.OfType<Unit>();
            foreach (var villager in villagers)
            {
                Assert.True(villager.Focused);

                var pathCycle = villager
                    .GetPrivateMemberValue<LinkedList<(Point point, Sprite target)>, Unit>("_pathCycle");
                var currentPathTarget = villager
                    .GetPrivateMemberValue<LinkedListNode<(Point point, Sprite target)>, Unit>("_currentPathTarget");
                var isPathLoop = villager
                    .GetPrivateMemberValue<bool, Unit>("_isPathLoop");

                Assert.False(isPathLoop);
                Assert.Null(currentPathTarget.Value.target);
                Assert.Equal(blankPoint, currentPathTarget.Value.point);

                Assert.Single(pathCycle);
                var current = pathCycle.First;
                Assert.Null(current.Value.target);
                Assert.Equal(blankPoint, current.Value.point);
            }
        }*/
    }
}
