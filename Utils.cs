using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media;
using System.Windows.Forms;

namespace Tetris {
  class Coords {
    public int X;
    public int Y;
    public int Z;

    public Coords( int X, int Y=0, int Z=0 ) {
      this.X = X;
      this.Y = Y;
      this.Z = Z;
    }
  }

  class WavPlayer {
    MediaPlayer _Player = new MediaPlayer();
    bool _Repeat = false;

    public WavPlayer( string relativePath ) {
      _Player.Open( new Uri( Path.GetFullPath( relativePath ) ) );
      _Player.MediaEnded += new EventHandler( ( object obj, EventArgs e ) => {
        MediaPlayer player = (obj as MediaPlayer);

        if ( _Repeat )
          player.Position = TimeSpan.Zero;
      } );
    }

    public void Play( bool repeat=false ) {
      _Repeat = repeat;

      _Player.Position = TimeSpan.Zero;
      _Player.Play();
    }
    public void Stop() {
      _Player.Stop();
    }
    public void ToggleMute() {
      _Player.IsMuted = !_Player.IsMuted;
    }
  }

  class FileIO {
    StreamWriter _StreamWriter;
    StreamReader _StreamReader;
    OpenFileDialog _FileDialog = new OpenFileDialog();

    public FileIO() {
      _FileDialog.Filter = "csv files (*.csv)|*.csv";
    }

    public void Save( string path, String line ) => Save( path, new List<string>() { line } );
    public void Save( string path, List<String> lines ) {
      _StreamWriter = new StreamWriter( path );

      foreach ( string line in lines )
        _StreamWriter.WriteLine( line );

      _StreamWriter.Close();
    }
    public List<String> ReadLines( string path ) {
      return Read( path ).Split( "\n" ).ToList();
    }
    public string Read( string path ) {
      string file = "";

      if ( File.Exists( path ) ) {
        _StreamReader = new StreamReader( path );

        file = _StreamReader.ReadToEnd();

        _StreamReader.Close();
      }

      return file;
    }
    public string Search() {
      if ( _FileDialog.ShowDialog() == DialogResult.OK )
        return _FileDialog.FileName;
      else
        return "";
    }
  }
}