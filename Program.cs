using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Tetris {
  partial class MainForm : Form {
    public enum GameState {
      SettingData,
      WaitingForPlayer,
      Playing,
      Pause,
      GameOver
    };

    public string Nickname { get; private set; } = "";
    public bool [] Keys { get; private set; } = new bool[ 225 ];
    public int [] KeysTimers { get; private set; } = new int[ 225 ];

    WavPlayer _Player_GameOver = new WavPlayer( "./wav/game_over.wav" );
    WavPlayer _Player_LevelUp = new WavPlayer( "./wav/level_up.wav" );
    WavPlayer _Player_Warning = new WavPlayer( "./wav/warning.wav" );
    WavPlayer _Player_Music1 = new WavPlayer( "./wav/music-lvl-1.wav" );
    WavPlayer _Player_Music2 = new WavPlayer( "./wav/music-lvl-2.wav" );

    TextArea _Ui_Instruction;
    Scoreboard _Ui_Scoreboard;
    Input _Ui_Nickname;
    Menu _Ui_Menu;
    Menu _Ui_BottomMenu;
    Box _Ui_GameBox;

    Color _Color_ButtonHover = Color.FromArgb( 20, 200, 200, 200 );
    Color _Color_Background = Color.FromArgb( 255, 0, 35, 44 );
    Color _Color_Main = Color.FromArgb( 100, 255, 255, 255 );

    Level _Level;
    Timer _LogicTimer;
    Timer _DrawTimer;
    FileIO _FileIO = new FileIO();
    GameState _State = GameState.SettingData;
    string _nicknamePrefix = "(You) ";
    string _PauseTemp = "Pause";
    bool _ResetInterval = false;
    int _KeyInterval = 10;
    int _SpeedLevel;
    int _Interval;

    int _Size_WidthForm;
    int _Size_WidthGameBox = 300;
    int _Size_WidthScoreboard = 250;
    int _Size_WidthInstructions = 250;
    int _Size_HeightInitial = 222;
    int _Size_HeightMenu = 30;
    int _Size_Brick = 20;

    [STAThread]
    public static void Main() {
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault( false );

      Application.Run( new MainForm() );
    }

    public MainForm() {
      _Size_WidthForm = 10 + _Size_WidthInstructions + 10 + _Size_WidthGameBox + 10 + _Size_WidthScoreboard + 10;

      InitializeComponent( _Size_WidthForm, _Size_HeightInitial + _Size_HeightMenu * 2 + 20, "Tetris" );

      _InitMenu();
      _InitInstructions();
      _InitGamebox();
      _InitScoreboard();
      _InitBottomMenu();
      _InitNickname();
      _InitTimers();

      KeyPreview = true;
      BackColor = _Color_Background;
      FormBorderStyle = FormBorderStyle.None;
      KeyDown += new KeyEventHandler( (object sender, KeyEventArgs e) => Keys[ (int) e.KeyCode ] = true );
      KeyUp += new KeyEventHandler( (object sender, KeyEventArgs e) => Keys[ (int) e.KeyCode ] = false );

      Controls.Add( _Ui_Menu );
      Controls.Add( _Ui_Nickname );
      Controls.Add( _Ui_Instruction );
      Controls.Add( _Ui_Scoreboard );
      Controls.Add( _Ui_BottomMenu );
      Controls.Add( _Ui_GameBox );
    }

    public void Play( int x, int y, int speed ) {
      if ( _State == GameState.SettingData )
        return;

      _Level = new Level( x, y );
      _Interval = speed;
      _SpeedLevel = 1;
      _LogicTimer.Interval = _Interval;

      _Player_Music1.Play( true );

      _PauseTemp = "Pause";
      _Ui_GameBox.Label = "Tetris";
      _State = GameState.Playing;

      _UpdateSize();
    }
    public void Pause() {
      string temp = _Ui_GameBox.Label;

      _Ui_GameBox.Label = _PauseTemp;
      _PauseTemp = temp;

      _State = GameState.Pause;
    }
    public void Resume() {
      string temp = _Ui_GameBox.Label;

      _Ui_GameBox.Label = _PauseTemp;
      _PauseTemp = temp;

      _State = GameState.Playing;
    }
    public void ToggleState() {
      if ( _State == GameState.Playing )
        Pause();
      else if ( _State == GameState.Pause )
        Resume();
    }
    public void CloseApp() {
      Application.Exit();
    }
    public void End() {
      _State = GameState.GameOver;

      _Player_GameOver.Play();
      _Player_Music1.Stop();
      _Player_Music2.Stop();
    }

    void _InitInstructions() {
      _Ui_Instruction = new TextArea(
        _Color_Main,
        _Color_Main,

        ""
        + "\n [ p ]:  Start/pause"
        + "\n [ r ]:  Reset the game"
        + "\n [ q ]:  Close the app"
        + "\n [ a | ← ]:  Move to left"
        + "\n [ d | → ]:  Move to right"
        + "\n [ w | ↑ ]:  Rotate"
        + "\n [ s | ↓ ]:  Move down"
        + "\n [ space ]: Drop to the bottom"
        + "\n"
        + "\n Scoreboard overriding "
        + "\n occurs after save;"
        + "\n Save the scoreboard before "
        + "\n changing the nickname;"
        + "\n Music is playing while gameplay"
        + "\n (the menu is quiet)",

        "Instruction",
        10,
        _Size_HeightMenu + 10,
        _Size_WidthInstructions,
        _Size_HeightInitial
      );
    }
    void _InitGamebox() {
      _Ui_GameBox = new Box(
        _Color_Main,
        "Tetris",
        10 + _Size_WidthInstructions + 10,
        _Size_HeightMenu + 10,
        _Size_WidthGameBox,
        _Size_HeightInitial
      );
    }
    void _InitScoreboard() {
      _Ui_Scoreboard = new Scoreboard(
        new List<DataArea.Item>(),
        _Color_Main,
        _Color_Main,
        "Scoreboard",
        10 + _Size_WidthInstructions + 10 + _Size_WidthGameBox + 10,
        _Size_HeightMenu + 10,
        _Size_WidthScoreboard,
        _Size_HeightInitial
      );

      _ReadScoreboard( "./scoreboard.csv" );
    }
    void _InitBottomMenu() {
      _Ui_BottomMenu = new Menu(
        this,
        0,
        _Size_HeightMenu + 10 + _Ui_Scoreboard.Height + 10,
        _Size_WidthForm,
        _Size_HeightMenu,
        Tetris.Menu.BorderPos.Top
      );

      // _Ui_BottomMenu.AddRadioRight(
      //   () => _ToggleMusic(),
      //   true, _Color_Main, Color.Transparent, _Color_ButtonHover, "Muzyka"
      // );
      _Ui_BottomMenu.AddBtnRight(
        () => _ToggleMusic(),
        _Color_Main, Color.Transparent, _Color_ButtonHover, "Toggle music"
      );

      _Ui_BottomMenu.AddBtnLeft(
        () => Play( 10, 20, 500 ),
        _Color_Main, Color.Transparent, _Color_ButtonHover, "Standard game"
      );
      _Ui_BottomMenu.AddBtnLeft(
        () => Play( 10, 10, 1000 ),
        _Color_Main, Color.Transparent, _Color_ButtonHover, "The square"
      );
      _Ui_BottomMenu.AddBtnLeft(
        () => Play( 30, 10, 1000 ),
        _Color_Main, Color.Transparent, _Color_ButtonHover, "Fat fun"
      );
      _Ui_BottomMenu.AddBtnLeft(
        () => Play( 15, 25, 100 ),
        _Color_Main, Color.Transparent, _Color_ButtonHover, "No time"
      );
    }
    void _InitNickname() {
      _Ui_Nickname = new Input(
        _Color_Background,
        _Color_Main,
        "Enter the nick and press enter",
        10 + _Size_WidthInstructions + 10 + _Ui_GameBox.Width / 2,
        _Size_HeightMenu + 10 + _Size_HeightInitial / 2,
        200,
        30
      );

      _Ui_Nickname.Left -= _Ui_Nickname.Width / 2;
      _Ui_Nickname.Top -= _Ui_Nickname.Height / 2;

    }
    void _InitMenu() {
      _Ui_Menu = new Menu( this, 0, 0, _Size_WidthForm, _Size_HeightMenu, Tetris.Menu.BorderPos.Bottom );
      _Ui_Menu.AddTitle( _Color_Main, Nickname );

      _Ui_Menu.AddBtnRight(
        () => CloseApp(),
        _Color_Main, Color.Transparent, Color.FromArgb( 255, 180, 0, 0 ), "[ q ]"
      );

      _Ui_Menu.AddBtnLeft(
        () => _SaveScoreboard(),
        _Color_Main, Color.Transparent, _Color_ButtonHover, "Save scoreboard"
      );
      _Ui_Menu.AddBtnLeft(
        () => _ReadScoreboard(),
        _Color_Main, Color.Transparent, _Color_ButtonHover, "Load scoreboard"
      );
      _Ui_Menu.AddBtnLeft(
        () => _Ui_Scoreboard.RemoveItem( Nickname ),
        _Color_Main, Color.Transparent, _Color_ButtonHover, "Remove scores of your nick"
      );
      _Ui_Menu.AddBtnLeft(
        () => _ChangeNickname(),
        _Color_Main, Color.Transparent, _Color_ButtonHover, "Rename"
      );
    }
    void _InitTimers() {
      _LogicTimer = new Timer() { Interval=500, Enabled=true };
      _LogicTimer.Tick += new System.EventHandler( (object sender, EventArgs e) => _Logic() );

      _DrawTimer = new Timer() { Interval=100, Enabled=true };
      _DrawTimer.Tick += new System.EventHandler( (object sender, EventArgs e) => _Draw() );

      _DrawTimer = new Timer() { Interval=5, Enabled=true };
      _DrawTimer.Tick += new System.EventHandler( (object sender, EventArgs e) => _KeyActions() );
    }

    bool _Key( int keyCode ) {
      return Keys[ keyCode ] && KeysTimers[ keyCode ] == 0;
    }
    void _KeyActions() {
      for ( int i = KeysTimers.Length - 1;  i >= 0;  --i )
        if ( KeysTimers[ i ] > 0 )
          --KeysTimers[ i ];

      if ( _State == GameState.SettingData ) {
        if ( _Key( 13 ) && _Ui_Nickname.Text != "" ) { // enter
          Nickname = _Ui_Nickname.Text;

          _State = GameState.WaitingForPlayer;
          // _menu.Title = "Grasz jako: " + Nickname;
          _Ui_Scoreboard.AddItem( _nicknamePrefix + Nickname, "0" );
          _Ui_Nickname.Hide();
        }
        else
          return;
      }

      if ( _Key( 80 ) ) { // p
        KeysTimers[ 80 ] = _KeyInterval;
        ToggleState();
      }

      if ( _Key( 81 ) ) { // q
        KeysTimers[ 81 ] = _KeyInterval;
        Close();
      }

      if ( _Key( 82 ) ) { // r
        KeysTimers[ 82 ] = _KeyInterval;
        Play( _Level.Width, _Level.Height, _Interval );
      }

      if ( _State == GameState.Pause )
        return;

      if ( _Key( 38 ) || _Key( 87 ) ) { // up || w
        KeysTimers[ 38 ] = _KeyInterval;
        KeysTimers[ 87 ] = _KeyInterval;
        _Level.Rotate();
      }

      if ( _Key( 40 ) || _Key( 83 ) ) { // down || s
        KeysTimers[ 40 ] = (int) (_KeyInterval * .6);
        KeysTimers[ 83 ] = (int) (_KeyInterval * .6);
        _LogicTimer.Interval = 1;
        _ResetInterval = true;
      }

      if ( _Key( 32 ) ) { // space
        while ( true ) {
          List<List<Brick>> fallingBricks = _Level
            .GetAllBig()
            .Where( b => b.Count > 0 && b[ 0 ].State == Brick.BrickState.Dynamic && b[ 0 ].Sterable )
            .ToList();

          if ( fallingBricks.Count > 0 )
            _Level.Jump();
          else
            break;
        }
      }

      if ( _Key( 37 ) || _Key( 65 ) ) { // left || a
        KeysTimers[ 37 ] = (int) (_KeyInterval * .7);
        KeysTimers[ 65 ] = (int) (_KeyInterval * .7);
        _Level.Move(
          _Level
            .GetAllBig()
            .Where( b => b.Count > 0 && b[ 0 ].State == Brick.BrickState.Dynamic && b[ 0 ].Sterable )
            .ToList(),
          -1,
          0
        );
      }

      if ( _Key( 39 ) || _Key( 68 ) ) { // right || d
        KeysTimers[ 39 ] = (int) (_KeyInterval * .7);
        KeysTimers[ 68 ] = (int) (_KeyInterval * .7);
        _Level.Move(
          _Level
            .GetAllBig()
            .Where( b => b.Count > 0 && b[ 0 ].State == Brick.BrickState.Dynamic && b[ 0 ].Sterable )
            .ToList(),
          1,
          0
        );
      }
    }
    void _UpdateSize() {
      _Ui_GameBox.UpdateSize( _Level.Width * _Size_Brick, _Level.Height * _Size_Brick );

      _Ui_Scoreboard.Left = 10 + _Size_WidthInstructions + 10 + _Ui_GameBox.Width + 10;

      Width = 10 + _Ui_Instruction.Width + 10 + _Ui_GameBox.Width + 10 + _Ui_Scoreboard.Width + 10;
      Height = _Ui_Menu.Height + 10 + _Ui_GameBox.Height + 10 + _Ui_BottomMenu.Height;

      _Ui_Menu.Width = Width;
      _Ui_Menu.Invalidate();

      _Ui_BottomMenu.Width = Width;
      _Ui_BottomMenu.Top = _Ui_Menu.Height + 10 + _Ui_GameBox.Height + 10;
      _Ui_BottomMenu.Invalidate();

      _Ui_Nickname.Left = 10 + _Ui_Instruction.Width + 10 + _Ui_GameBox.Width / 2 - _Ui_Nickname.Width / 2;
    }
    void _SaveScoreboard() {
      List<string> lines = new List<string>();

      foreach ( DataArea.Item item in _Ui_Scoreboard.Items )
        if ( item.Name != Nickname ) {
          if ( item.Name == _nicknamePrefix + Nickname )
            lines.Add( Nickname + ";" + item.Value );
          else
            lines.Add( item.Name + ";" + item.Value );
        }

      _FileIO.Save( "./scoreboard.txt", lines );
    }
    void _ReadScoreboard() => _ReadScoreboard( _FileIO.Search() );
    void _ReadScoreboard( string path ) {
      if ( path == "" )
        return;

      _Ui_Scoreboard.ClearItems();

      foreach ( string line in _FileIO.ReadLines( path ) ) {
        string [] lineData = line.Split( ";" );

        // string name = lineData[ 0 ];
        // string value = lineData[ 1 ];

        if ( lineData.Length == 2 )
          _Ui_Scoreboard.AddItem( lineData[ 0 ], lineData[ 1 ] );
      }
    }
    void _ChangeNickname() {
      _Ui_Scoreboard.RemoveItem( _nicknamePrefix + Nickname );

      _State = GameState.SettingData;
      _Level = null;

      _Player_Music1.Stop();
      _Player_Music2.Stop();

      _Ui_Nickname.Show();

      _ReadScoreboard( "./scoreboard.csv" );
    }
    void _ToggleMusic() {
      _Player_Music1.ToggleMute();
      _Player_Music2.ToggleMute();
    }
    void _Logic() {
      if ( _State != GameState.Playing )
        return;

      if ( _Level.Score / _SpeedLevel > 50 ) {
        ++_SpeedLevel;
        _Interval = (int) (_Interval * 0.8);

        _Player_LevelUp.Play();

        if ( _SpeedLevel == 3 )
          _Player_Warning.Play();
        else if ( _SpeedLevel == 4 ) {
          _Player_Music1.Stop();
          _Player_Music2.Play( true );
        }
      }

      if ( _ResetInterval )
        _LogicTimer.Interval = _Interval;

      _Level.Jump();
      _Level.DestroyRows();

      int countOfSterable = _Level
        .GetAllBig()
        .Where( b => b.Count > 0 && b[ 0 ].State == Brick.BrickState.Dynamic && b[ 0 ].Sterable )
        .Count();

      int bricksToSpawn = countOfSterable == 0  ?  1  :  0;

      for ( int i = bricksToSpawn;  i > 0;  i-- )
        if ( !_Level.GenerateBrick( "user" ) ) //, Level.BrickType.Rect2x2
          End();
    }
    void _Draw() {
      _Ui_GameBox.Clear();

      if ( _Level == null )
        return;

      int width = _Ui_GameBox.DrawingWidth / _Level.Width;
      int height = _Ui_GameBox.DrawingHeight / _Level.Height;

      foreach ( Brick brick in _Level.GetAll() )
        _Ui_GameBox.FillRectangle( brick.Color, width * brick.X, height * brick.Y, width, height );

      _Ui_Scoreboard.ChangeItemValue( _nicknamePrefix + Nickname, "" + (_Level.Score * 10) );
    }
  }
}
