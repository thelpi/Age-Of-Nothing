using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Age_Of_Nothing;
using Age_Of_Nothing.Sprites;
using Xunit;

namespace Age_Of_Nothing_Unit_Tests.ControllerTests
{
    public class SetTargetPositionsOnFocusedTests
    {
        [Fact]
        public void SetTargetPositionsOnFocused_WhenSourceAreVillagers_WhenTargetIsMine_WhenMarketExists_CreateCycle()
        {
            var controller = new Controller();
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
        }
    }
}
