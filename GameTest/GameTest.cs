using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Game_Xamarin.Model;
using Game_Xamarin.Persistence;
using Moq;
using System.Threading.Tasks;

namespace Game_Xamarin.GameTest
{
    [TestClass]
    public class GameTest
    {
        private GameModel _model;
        private GameInfos _mockedInfos;
        private Mock<IGameDataAccess> _mock;


        private void Initialize(int tableSize)
        {
            _mockedInfos = new GameInfos(tableSize);

            _mock = new Mock<IGameDataAccess>();
            _mock.Setup(mock => mock.LoadAsync(It.IsAny<String>()))
                .Returns(() => Task.FromResult(_mockedInfos));

            _model = new GameModel(_mock.Object);
            _model.NewGame(_mockedInfos);

            _model.OnGameOver += new EventHandler<GameOverEventArgs>(Model_GameOver);
            //_model.OnBasketFound += new EventHandler(Model_BasketFound);

        }

        [TestMethod]
        public void GameModelNewGameEasyTest()
        {
            Initialize(12);
            Assert.AreEqual(0, _model.BasketCount); // m�g egy kosarat sem gy�jt�tt�nk
            Assert.AreEqual(new Coord(0, 0), _model.GameInfos.PlayerCoords); // j�t�kos kezdetben a 0,0 koordin�t�n �ll

            int emptyFields = (int)Math.Pow(_model.GetGeneratedFieldCountEasy, 2) - _model.GameInfos.Hunters.Count -
                _model.GameInfos.Trees.Count - _model.GameInfos.Baskets.Count - 1;

            Assert.AreEqual(125, emptyFields); // szabad mez�k sz�ma is megfelel�
        }

        [TestMethod]
        public void GameModelNewGameMediumTest()
        {
            Initialize(18);
            Assert.AreEqual(0, _model.BasketCount);
            Assert.AreEqual(new Coord(0, 0), _model.GameInfos.PlayerCoords);

            int emptyFields = (int)Math.Pow(_model.GetGeneratedFieldCountMedium, 2) - _model.GameInfos.Hunters.Count -
                _model.GameInfos.Trees.Count - _model.GameInfos.Baskets.Count - 1;

            Assert.AreEqual(296, emptyFields); // szabad mez�k sz�ma is megfelel�
        }

        [TestMethod]
        public void GameModelNewGameHardTest()
        {
            Initialize(24);
            Assert.AreEqual(0, _model.BasketCount);
            Assert.AreEqual(new Coord(0, 0), _model.GameInfos.PlayerCoords);

            int emptyFields = (int)Math.Pow(_model.GetGeneratedFieldCountHard, 2) - _model.GameInfos.Hunters.Count -
                _model.GameInfos.Trees.Count - _model.GameInfos.Baskets.Count - 1;

            Assert.AreEqual(539, emptyFields); // szabad mez�k sz�ma is megfelel�
        }

        [TestMethod]
        public void GameModelPlayerMoveTest()
        {
            Initialize(12);
            Assert.AreEqual(new Coord(0, 0), _model.GameInfos.PlayerCoords);
            _model.PlayerMove(Direction.Up);
            Assert.AreEqual(new Coord(0, 0), new Coord(_model.Player.X, _model.Player.Y)); // mivel a bal fels� sarokban �ll nem v�ltozik a koordin�ta
            _model.PlayerMove(Direction.Left);
            Assert.AreEqual(new Coord(0, 0), new Coord(_model.Player.X, _model.Player.Y)); // balra sem l�phet
            _model.PlayerMove(Direction.Right);
            Assert.AreEqual(new Coord(1, 0), new Coord(_model.Player.X, _model.Player.Y)); // ell�pett egyel jobra
            _model.PlayerMove(Direction.Right);
            Assert.AreEqual(new Coord(2, 0), new Coord(_model.Player.X, _model.Player.Y)); //ell�pett m�g egyel jobbra
            _model.PlayerMove(Direction.Down);
            Assert.AreEqual(new Coord(2, 0), new Coord(_model.Player.X, _model.Player.Y)); // mivel f�ba �tk�zne ez�rt nem v�ltozik a koordin�ta
        }

        [TestMethod]
        public void GameModelHuntersMoveTest()
        {
            Initialize(12);
            foreach (Hunter hunter in _model.GameInfos.Hunters)
            {
                Hunter previoustPosition = new Hunter(hunter.CoordX, hunter.CoordY, hunter.Direction); // lementj�k az el�z� poz�ci�t
                hunter.Move(_model.GameInfos.Trees, 12); // l�ptetj�k egyel a vad�szt a megfelel� ir�ny�ba
                Hunter currentPosition = new Hunter(hunter.CoordX, hunter.CoordY, hunter.Direction); // lementj�k a jelenlegi poz�ci�j�t
                Assert.IsFalse(_model.GameInfos.Hunters.Contains(previoustPosition));
                Assert.IsTrue(_model.GameInfos.Hunters.Contains(currentPosition)); // megn�zz�k, hogy az el�z� poz�ci�j�r�l t�nyleg ell�pett
            }

        }

        [TestMethod]
        public void GameModelCheckBasketFoundTest()
        {
            Initialize(12);
            _model.PlayerMove(Direction.Right);
            _model.PlayerMove(Direction.Right);
            _model.PlayerMove(Direction.Right);
            _model.PlayerMove(Direction.Right);
            _model.PlayerMove(Direction.Down); // felvessz�k az egyik kosarat

            Assert.AreEqual(5, _model.GameInfos.Baskets.Count);
            Assert.AreEqual(1, _model.BasketCount);
        }

        [TestMethod]
        public void GameModelCheckNoHuntersAroundPlayerTest()
        {
            Initialize(12);
            _model.PlayerMove(Direction.Down);
            _model.PlayerMove(Direction.Down);
            _model.PlayerMove(Direction.Down);
            //ezeknek k�sz�nhet�en kiv�lt�dik az esem�ny, hogy megl�tott egy vad�sz �s elvesztett�k a j�t�kot

        }

        private void Model_GameOver(object sender, GameOverEventArgs e)
        {
            Assert.IsTrue(_model.GameOver); //v�ge a j�t�knak
            Assert.AreEqual(new Hunter(2, 3, Direction.Right), e.Hunter); //ez a vad�sz l�tta meg a j�t�kost
            Assert.IsFalse(e.IsWon); // nem nyert a j�t�kos
        }

        [TestMethod]
        public void GameModelCheckHunterAndBasketOnSameFieldTest()
        {
            Initialize(12);
            for (int i = 0; i < 5; ++i)
            {
                _model.HuntersMove();
            }

            bool areHuntersOnBasket = false;
            int numbersOfHunters = 0;

            foreach (Hunter hunter in _model.GameInfos.Hunters)
            {
                bool thisHunterOnSameField = _model.CheckHunterAndBasketOnSameField(hunter);
                areHuntersOnBasket = areHuntersOnBasket || thisHunterOnSameField;
                if (thisHunterOnSameField)
                {
                    ++numbersOfHunters;
                }

            }

            Assert.IsTrue(areHuntersOnBasket); // kell lennie 2 vad�sznak is olyan mez�n hol kos�r van
            Assert.AreEqual(2, numbersOfHunters);

            _model.HuntersMove();
            areHuntersOnBasket = false;
            numbersOfHunters = 0;

            foreach (Hunter hunter in _model.GameInfos.Hunters)
            {
                bool thisHunterOnSameField = _model.CheckHunterAndBasketOnSameField(hunter);
                areHuntersOnBasket = areHuntersOnBasket || thisHunterOnSameField;
                if (thisHunterOnSameField)
                {
                    ++numbersOfHunters;
                }

            }

            Assert.IsFalse(areHuntersOnBasket); // mostm�r egy vad�sznak sem szabad azonos mez�n lennie egy kos�rral
            Assert.AreEqual(0, numbersOfHunters);
        }

        [TestMethod]
        public async Task GameModelLoadNewGameTest()
        {
            Initialize(12);
            await _model.LoadNewGameAsync(string.Empty);

            foreach (Coord tree in _model.GameInfos.Trees)
            {
                Assert.IsTrue(_mockedInfos.Trees.Contains(tree));
                //ellen�rizz�k, hogy valamennyi fa megfelel�-e
            }

            foreach (Hunter hunter in _model.GameInfos.Hunters)
            {
                Assert.IsTrue(_mockedInfos.Hunters.Contains(hunter));
                //ellen�rizz�k, hogy valamennyi vad�sz megfelel�-e
            }

            foreach (Coord basket in _model.GameInfos.Baskets)
            {
                Assert.IsTrue(_mockedInfos.Baskets.Contains(basket));
                //ellen�rizz�k, hogy valamennyi kos�r megfelel�-e
            }
        }
    }
}
