using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Game_Xamarin.View;
using Game_Xamarin.Model;
using Game_Xamarin.Persistence;
using Game_Xamarin.ViewModel;
using System.Threading.Tasks;

namespace Game_Xamarin
{
    
    public partial class App : Application
    {

        private IGameDataAccess _gameDataAccess;
        private GameModel _model;
        private GameViewModel _viewModel;
        private GamePage _gamePage;

        //private StoredGameBrowserModel _storedGameBrowserModel;
        //private StoredGameBrowserViewModel _storedGameBrowserViewModel;

        private bool _advanceTimer;
        public App()
        {
            InitializeComponent();

            //MainPage = new GamePage();
            _gameDataAccess = DependencyService.Get<IGameDataAccess>();

            _model = new GameModel(_gameDataAccess);
            _model.OnGameOver += new EventHandler<GameOverEventArgs>(GameModel_GameOver);

            _viewModel = new GameViewModel(_model);
            _viewModel.NewGameEasy += new EventHandler(GameViewModel_NewGameEasy);
            _viewModel.NewGameMedium += new EventHandler(GameViewModel_NewGameMedium);
            _viewModel.NewGameHard += new EventHandler(GameViewModel_NewGameHard);
            _viewModel.PauseGame += new EventHandler(GameViewModel_PauseGame);

            _gamePage = new GamePage();
            _gamePage.BindingContext = _viewModel;

            MainPage = new NavigationPage(_gamePage);
            _advanceTimer = false;
        }

        protected override void OnStart()
        {
            // Először még semmilyen pályát nem kell betöltenie
        }

        protected override void OnSleep()
        {
            if (!_model.Paused)
            {
                _model.PauseGame();
                _viewModel.ContinueText = "Folytatás";
                _advanceTimer = false;
            }
                
        }

        protected override void OnResume()
        {
            // megállítottuk mikor alvó üzemmódba került, a játékos visszalépve eldöntheti, mikor folytatja
            //if (_model.Paused)
            //{
            //    _model.ContinueGame();
            //    _advanceTimer = true;
            //}
        }


        private void GameViewModel_PauseGame(object sender, EventArgs e)
        {
            if (!_model.Paused)
            {
                _model.PauseGame();
                _viewModel.ContinueText = "Folytatás";
                _advanceTimer = false;
            }
            else
            {
                _model.ContinueGame();
                _viewModel.ContinueText = "Szünet";
                _advanceTimer = true;
                Device.StartTimer(TimeSpan.FromSeconds(1), () => { if (!_model.Paused && !_model.GameOver) { _model.HuntersMove(); } return _advanceTimer; });
            }
        }

        private async void GameViewModel_NewGameEasy(object sender, EventArgs e)
        {
            _model.PauseGame();
            await Initialize(_model.GetGeneratedFieldCountEasy, "EasyMode.txt");
            _model.ContinueGame();
            Device.StartTimer(TimeSpan.FromSeconds(1), () => { if (!_model.Paused && !_model.GameOver) { _model.HuntersMove(); } return _advanceTimer; });
        }

        private async void GameViewModel_NewGameMedium(object sender, EventArgs e)
        {
            _model.PauseGame();
            await Initialize(_model.GetGeneratedFieldCountMedium, "MediumMode.txt");
            _model.ContinueGame();
            _advanceTimer = true;
            Device.StartTimer(TimeSpan.FromSeconds(1), () => { if (!_model.Paused && !_model.GameOver) { _model.HuntersMove(); } return _advanceTimer; });
        }

        private  async void GameViewModel_NewGameHard(object sender, EventArgs e)
        {
            _model.PauseGame();
            await Initialize (_model.GetGeneratedFieldCountHard, "HardMode.txt");
            _model.ContinueGame();
            Device.StartTimer(TimeSpan.FromSeconds(1), () => { if (!_model.Paused && !_model.GameOver) { _model.HuntersMove(); } return _advanceTimer; });
        }

        private async void GameModel_GameOver(object sender, GameOverEventArgs e)
        {
            _advanceTimer = false;

            if (e.IsWon)
            {
                await MainPage.DisplayAlert("Játék vége", "Gratulálok, nyertél!" + Environment.NewLine
                    + "Összegyűjtötted mind a(z) " + _viewModel.BasketCount + " kosarat és összesen " + _viewModel.GameTime + " ideig játszottál!",
                    "OK");
            }
            else
            {
                await MainPage.DisplayAlert("Játék vége", "Vesztettél!" + Environment.NewLine
                    + "A(z) (" + (e?.Hunter.CoordX + 1) + "," + (e?.Hunter.CoordY + 1) + ") koordinátán lévő vadász meglátott!","OK");
            }
            _viewModel.ContinueEnabled = !_model.GameOver;
        }

        private async Task Initialize(int startSize, string path)
        {
            _advanceTimer = false;
            _viewModel.ContinueText = "Szünet";
            _viewModel.GenerateTable(startSize);
            await _model.LoadNewGameAsync(path);
            _viewModel.ContinueEnabled = !_model.GameOver;
            _viewModel.RegisterPlayer();
            _viewModel.RegisterHunter();
            _viewModel.DrawDefaultGame();
            _advanceTimer = true;
        }
    }
}
