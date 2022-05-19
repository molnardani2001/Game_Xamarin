using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Game_Xamarin.Model;
using Game_Xamarin.Persistence;
using System.Diagnostics;

namespace Game_Xamarin.ViewModel
{
    public class GameViewModel : ViewModelBase
    {
        #region fields

        private GameModel _model; //üzleti logika
        private String _continueText = "Szünet"; //szünet gomb felirata
        private bool _continueEnabled = false; //szünet gomb elérhetősége
        private int _height; //tábla magassága
        private int _width; //tábla szélessége
        private int _opacity = 0; // idő, kosár felirat átlátszósága
        private int _labelOpacity = 1; // kezdő szöveg átlátszósága

        #endregion

        #region properties

        // könnyű új játék kezdése parancs
        public DelegateCommand EasyNewGameCommand { get; private set; }

        // közepes új játék kezdése parancs
        public DelegateCommand MediumNewGameCommand { get; private set; }

        // nehéz új játék kezdése parancs
        public DelegateCommand HardNewGameCommand { get; private set; }
        
        // szünet/folytatás parancs
        public DelegateCommand PauseGameCommand { get; private set; }

        //balra lépés parancs
        public DelegateCommand StepLeftCommand { get; private set; }

        // jobbra lépés parancs
        public DelegateCommand StepRightCommand { get; private set; }

        //felfele lépés parancs
        public DelegateCommand StepUpCommand { get; private set; }
        
        //lefele lépés parancs
        public DelegateCommand StepDownCommand { get; private set; }

        // tábla mezőinek gyűjteménye
        public ObservableCollection<Field> Fields { get; private set; } = new ObservableCollection<Field>();

        // kosarak száma
        public Int32 BasketCount 
        { 
            get 
            {   
                if(_model.GameInfos == null)
                {
                    return 0;
                }
                else
                {
                    return _model.BasketCount;
                }
                
            } 
        }

        // szünet/folytatás gomb elérhetősége
        public Boolean ContinueEnabled
        {
            get { return _continueEnabled; }
            set
            {
                _continueEnabled = value;
                OnPropertyChanged("ContinueEnabled");
            }
        }
        
        // idő, kosarak száma átlátszósága
        public Int32 Opacity
        {
            get
            {
                return _opacity;
            }
            set
            {
                _opacity = value;
                OnPropertyChanged();
            }
        }

        // üdvözlő szöveg átlátszósága
        public Int32 LabelOpacity
        {
            get
            {
                return _labelOpacity;
            }
            set
            {
                _labelOpacity = value;
                OnPropertyChanged();
            }
        }

        // szélesség
        public Int32 Width
        {
            get
            {
                return _width;
            }
            set
            {
                if(_width != value)
                {
                    _width = value;
                    OnPropertyChanged();
                }
            }
        }

        // magasság
        public Int32 Height
        {
            get
            {
                return _height;
            }
            set
            {
                if(_width != value)
                {
                    _height = value;
                    OnPropertyChanged();
                }
            }
        }

        // szünet/folytatás szöveg
        public String ContinueText
        {
            get { return _continueText; }

            set
            {
                if (_continueText != value)
                {
                    _continueText = value;
                    OnPropertyChanged();
                }
            }
        }

        // játékidő
        public String GameTime
        {
            get
            {
                if(_model.GameInfos == null)
                {
                    return "00:00:00";
                }
                else
                {
                    return TimeSpan.FromSeconds(Math.Floor(_model.ElapsedSeconds + (DateTime.Now - _model.StartTime).TotalSeconds)).ToString();
                }
                
            }
        }

        #endregion

        #region events

        public event EventHandler NewGameEasy; //új könnyű játék esemény
        public event EventHandler NewGameMedium; //új közepes játék esemény
        public event EventHandler NewGameHard;//új nehéz játék esemény

        public event EventHandler PauseGame; //játék megállítása/folytatása esemény

        #endregion

        #region constructors

        //üzleti logika bekötése, parancsok példányosítása
        public GameViewModel(GameModel model)
        {
            _model = model;
            //_model.OnGameOver += new EventHandler<GameOverEventArgs>(Model_GameOver);
            _model.OnBasketFound += new EventHandler(Model_BasketFound);
            _model.GameAdvenced += new EventHandler(Model_GameAdvanced);

            Height = Width = 0;

            EasyNewGameCommand = new DelegateCommand(param => OnNewGameEasy());
            MediumNewGameCommand = new DelegateCommand(param => OnNewGameMedium());
            HardNewGameCommand = new DelegateCommand(param => OnNewGameHard());

            PauseGameCommand = new DelegateCommand(param => OnPauseGame());

            StepDownCommand = new DelegateCommand(param => _model.PlayerMove(Direction.Down));
            StepUpCommand = new DelegateCommand(param => _model.PlayerMove(Direction.Up));
            StepLeftCommand = new DelegateCommand(param => _model.PlayerMove(Direction.Left));
            StepRightCommand = new DelegateCommand(param => _model.PlayerMove(Direction.Right));

        }
        #endregion

        #region event handlers

        //idő változás jelzése
        private void Model_GameAdvanced(object sender, EventArgs e)
        {
            OnPropertyChanged("GameTime");
        }

        //kosár találása jelzése
        private void Model_BasketFound(object sender, EventArgs e)
        {
            OnPropertyChanged("BasketCount");
        }

        //vadászok léptetése
        private void Hunter_HunterMove(object sender, MoveEventArgs e)
        {
            if (_model.GameOver || _model.Paused)
            {
                return;
            }
            Hunter hunter = sender as Hunter;
            int previousIndex = hunter.CoordY * _model.GameInfos.TableSize + hunter.CoordX;
            int nextIndex = e.NextPosition.Y * _model.GameInfos.TableSize + e.NextPosition.X;
            if (_model.CheckHunterAndBasketOnSameField(hunter))
            {
                Fields[previousIndex].Color = Color.Gold;
            }
            else
            {
                Fields[previousIndex].Color = Color.LightGreen;
            }
            Fields[nextIndex].Color = Color.Red;
        }

        //játékos léptetése
        private void Player_PlayerMove(object sender, MoveEventArgs e)
        {
            if (_model.GameOver || _model.Paused)
            {
                return;
            }
            Player player = sender as Player;
            int previousIndex = player.Y * _model.GameInfos.TableSize + player.X;
            int nextIndex = e.NextPosition.Y * _model.GameInfos.TableSize + e.NextPosition.X;
            Fields[previousIndex].Color = Color.LightGreen;
            Fields[nextIndex].Color = Color.Brown;
        }

        #endregion

        #region methods (helper)

        //tábla mezőinek generálása adott méretre
        public void GenerateTable(int startSize)
        {
            Fields.Clear();
            Width = Height = startSize;
            for (Int32 i = 0; i < startSize; i++)
            {
                for (Int32 j = 0; j < startSize; j++)
                {
                    Fields.Add(new Field
                    {
                        Color = Color.LightGreen,
                        X = i,
                        Y = j,
                        Number = i * startSize + j
                    });
                }
            }
        }

        //vadászok eseménykezelőinek feliratkoztatása
        public void RegisterHunter()
        {
            foreach(Hunter hunter in _model.GameInfos.Hunters)
            {
                hunter.OnHunterMove += Hunter_HunterMove;
            }
        }

        //játékos eseménykezelőjének feliratkoztatása
        public void RegisterPlayer()
        {
            _model.Player.OnPlayerMove += Player_PlayerMove;
        }

        //mezők inicializálása
        public void DrawDefaultGame()
        {
            Fields[(_model.Player.Y * _model.GameInfos.TableSize) + _model.Player.X].Color = Color.Brown;

            foreach (Hunter hunter in _model.GameInfos.Hunters)
            {
                int index = hunter.CoordY * _model.GameInfos.TableSize + hunter.CoordX;
                Fields[index].Color = Color.Red;
            }

            foreach (Coord basket in _model.GameInfos.Baskets)
            {
                int index = basket.Y * _model.GameInfos.TableSize + basket.X;
                Fields[index].Color = Color.Gold;
            }

            foreach (Coord tree in _model.GameInfos.Trees)
            {
                int index = tree.Y * _model.GameInfos.TableSize + tree.X;
                Fields[index].Color = Color.Gray;
            }
            OnPropertyChanged("BasketCount");
            OnPropertyChanged("GameTime");
            //OnPropertyChanged("Fields");
            Opacity = 1;
            LabelOpacity = 0;
        }

        #endregion

        #region event methods

        private void OnPauseGame()
        {
            if(PauseGame != null)
            {
                PauseGame(this, EventArgs.Empty);
            }
        }

        private void OnNewGameEasy()
        {
            if(NewGameEasy != null)
            {
                NewGameEasy(this, EventArgs.Empty);
            }
        }

        private void OnNewGameMedium()
        {
            if (NewGameMedium != null)
            {
                NewGameMedium(this, EventArgs.Empty);
            }
        }

        private void OnNewGameHard()
        {
            if (NewGameHard != null)
            {
                NewGameHard(this, EventArgs.Empty);
            }
        }

        #endregion
    }
}
