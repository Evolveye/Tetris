using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using System.Windows.Forms;

namespace Tetris {
  class Level {
    public enum BrickType {
      Rect1x1,
      Rect2x2,
      Line4,
      L,
      Lr,
      Z,
      Zr,
      T
    };

    public float Score { get; private set; } = 0;
    public Brick [,] Bricks { get; private set; }
    public int Width {
      get => Bricks.GetLength( 1 );
    }
    public int Height {
      get => Bricks.GetLength( 0 );
    }

    Random _random = new Random();

    public Level( int width, int height ) => Bricks = new Brick[ height, width ];

    public bool IsEmpty( int x, int y ) {
      if ( x < 0 || x >= Width || y < 0 || y >= Height )
        return false;

      return Bricks[ y, x ] == null;
    }
    public Brick Get( int x, int y ) {
      if ( x < 0 || x >= Width || y < 0 || y >= Height )
        return null;

      return Bricks[ y, x ];
    }
    public List<Brick> GetAll() {
      List<Brick> bricks = new List<Brick>();

      foreach ( Brick brick in Bricks )
        if ( brick != null )
          bricks.Add( brick );

      return bricks;
    }
    public List<Brick> GetBig( int x, int y ) {
      List<Brick> bigBrick = new List<Brick>();

      _BuildBigBrick( bigBrick, x, y );

      bigBrick.Sort( (a, b) => a.X <= b.X && a.Y <= b.Y  ?  1  :  -1 );

      return bigBrick;
    }
    public List<List<Brick>> GetAllBig() {
      List<List<Brick>> bigBricks = new List<List<Brick>>();
      List<Coords> coords = new List<Coords>();

      for ( int y = Height - 1;  y >= 0;  y-- )
        for ( int x = Width - 1;  x >= 0;  x-- ) {
          if ( coords.Exists( c => c.X == x && c.Y == y ) )
            continue;

          List<Brick> bigBrick = new List<Brick>();

          _BuildBigBrick( bigBrick, x, y );

          foreach ( Brick brick in bigBrick )
            coords.Add( new Coords( brick.X, brick.Y ) );

          bigBricks.Add( bigBrick );
        }

      return bigBricks;
    }

    public bool GenerateBrick( string owner ) {
      int bricks = Enum.GetNames( typeof( BrickType ) ).Length;

      return GenerateBrick( owner, (BrickType) _random.Next( bricks ) );
    }
    public bool GenerateBrick( string owner, BrickType brickType ) {
      List<Coords> coords = new List<Coords>();

      switch ( brickType ) {
        case BrickType.Rect1x1:
          coords.Add( new Coords( 0, 0 ) );
          break;

        case BrickType.Rect2x2:
          coords.Add( new Coords( 0, 0 ) );
          coords.Add( new Coords( 1, 0 ) );
          coords.Add( new Coords( 0, 1 ) );
          coords.Add( new Coords( 1, 1 ) );
          break;

        // case BrickType.Line3:
        //   coords.Add( new Coords( 0, 0 ) );
        //   coords.Add( new Coords( 1, 0 ) );
        //   coords.Add( new Coords( 2, 0 ) );
        //   break;

        case BrickType.Line4:
          coords.Add( new Coords( 0, 0 ) );
          coords.Add( new Coords( 1, 0 ) );
          coords.Add( new Coords( 2, 0 ) );
          coords.Add( new Coords( 3, 0 ) );
          break;

        case BrickType.L:
          coords.Add( new Coords( 0, 0 ) );
          coords.Add( new Coords( 0, 1 ) );
          coords.Add( new Coords( 0, 2 ) );
          coords.Add( new Coords( 1, 2 ) );
          break;

        case BrickType.Lr:
          coords.Add( new Coords( 1, 0 ) );
          coords.Add( new Coords( 1, 1 ) );
          coords.Add( new Coords( 1, 2 ) );
          coords.Add( new Coords( 0, 2 ) );
          break;

        case BrickType.Z:
          coords.Add( new Coords( 0, 0 ) );
          coords.Add( new Coords( 1, 0 ) );
          coords.Add( new Coords( 1, 1 ) );
          coords.Add( new Coords( 2, 1 ) );
          break;

        case BrickType.Zr:
          coords.Add( new Coords( 0, 1 ) );
          coords.Add( new Coords( 1, 1 ) );
          coords.Add( new Coords( 1, 0 ) );
          coords.Add( new Coords( 2, 0 ) );
          break;

        case BrickType.T:
          coords.Add( new Coords( 0, 0 ) );
          coords.Add( new Coords( 1, 0 ) );
          coords.Add( new Coords( 2, 0 ) );
          coords.Add( new Coords( 1, 1 ) );
          break;
      }

      return Add( owner, coords );
    }
    public bool Add( string owner, List<Coords> coords ) {
      Color color = Brick.colors[ Brick.Random.Next( Brick.colors.Length ) ];

      int minX = coords[ 0 ].X;
      int maxX = coords[ 0 ].X;
      int minY = coords[ 0 ].Y;
      int maxY = coords[ 0 ].Y;

      foreach ( Coords cords in coords ) {
        if ( cords.X < minX )
          minX = cords.X;

        if ( cords.X > maxX )
          maxX = cords.X;

        if ( cords.Y < minY )
          minY = cords.Y;

        if ( cords.Y > maxY )
          maxY = cords.Y;
      }

      Coords center = new Coords( Width / 2 - (maxX - minX + 1) / 2, 0 );

      foreach ( Coords cords in coords )
        if ( !IsEmpty( cords.X + center.X, cords.Y ) )
          return false;

      foreach ( Coords cords in coords ) {
        bool borderLeft = false;
        bool borderRight = false;
        bool borderTop = false;
        bool borderBottom = false;

        foreach ( Coords crds in coords ) {
          if ( crds.X == cords.X - 1 && crds.Y == cords.Y )
            borderLeft = true;

          if ( crds.X == cords.X + 1 && crds.Y == cords.Y )
            borderRight = true;

          if ( crds.X == cords.X && crds.Y == cords.Y - 1 )
            borderTop = true;

          if ( crds.X == cords.X && crds.Y == cords.Y + 1 )
            borderBottom = true;
        }

        Bricks[ cords.Y, cords.X + center.X ] = new Brick(
          owner,
          cords.X + center.X,
          cords.Y,
          borderLeft,
          borderRight,
          borderTop,
          borderBottom,
          color
        );
      }

      return true;
    }
    public void Move( List<List<Brick>> bigBricksToMove, int jumpX, int jumpY ) {
      foreach ( List<Brick> bigBrick in bigBricksToMove.ToList() )
        foreach ( Brick brick in bigBrick ) {
          int newX = brick.X + jumpX;
          int newY = brick.Y + jumpY;

          if ( !IsEmpty( newX, newY ) ) {
            bool exists = false;

            foreach ( List<Brick> anotherBigBrick in bigBricksToMove )
              if ( anotherBigBrick.Exists( b => b.X == newX && b.Y == newY ) ) {
                exists = true;
                break;
              }

            if ( !exists ) {
              bigBricksToMove.RemoveAt( bigBricksToMove.IndexOf( bigBrick ) );
              break;
            }
          }
        }

      List<Brick> bricks = bigBricksToMove.SelectMany( c => c ).ToList();

      if ( jumpX >= 0 && jumpY >= 0 ) {
        bricks.Sort( (a, b) => b.X - a.X );
        bricks.Sort( (a, b) => b.Y - a.Y );
      }
      else if ( jumpX >= 0 && jumpY < 0 ) {
        bricks.Sort( (a, b) => b.X - a.X );
        bricks.Sort( (a, b) => a.Y - b.Y );
      }
      else if ( jumpX < 0 && jumpY >= 0 ) {
        bricks.Sort( (a, b) => a.X - b.X );
        bricks.Sort( (a, b) => b.Y - a.Y );
      }
      else { // ( jumpX < 0 && jumpY < 0 )
        bricks.Sort( (a, b) => a.X - b.X );
        bricks.Sort( (a, b) => a.Y - b.Y );
      }

      foreach ( Brick brick in bricks ) {
        int newX = brick.X + jumpX;
        int newY = brick.Y + jumpY;

        Bricks[ brick.Y, brick.X ] = null;

        Bricks[ newY, newX ] = brick;
        Bricks[ newY, newX ].X = newX;
        Bricks[ newY, newX ].Y = newY;
      }
    }
    public void Jump() {
      List<List<Brick>> bigBricks = GetAllBig()
        .Where( big => big.Count > 0 && big[ 0 ].State == Brick.BrickState.Dynamic )
        .ToList();

      foreach ( List<Brick> bigBrick in bigBricks )
        foreach ( Brick brick in bigBrick )
          if ( !IsEmpty( brick.X, brick.Y + 1 ) && !brick.BorderBottom ) {
            bool exists = false;

            foreach ( List<Brick> anotherBigBrick in bigBricks )
              if ( anotherBigBrick.Exists( b => b.X == brick.X && b.Y == brick.Y + 1 ) ) {
                exists = true;
                break;
              }

            if ( !exists ) {
              bool wasSterable = false;

              foreach ( Brick b in bigBrick ) {
                b.State = Brick.BrickState.Static;

                if ( b.Sterable ) {
                  wasSterable = true;
                  b.Sterable = false;
                }
              }

              if ( wasSterable )
                Score += (float) 1.5;

              break;
            }
          }

      Move( bigBricks, 0, 1 );
    }
    public void Rotate() {
      List<List<Brick>> sterableBricks = GetAllBig()
        .Where( b => b.Count > 0 && b[ 0 ].State == Brick.BrickState.Dynamic && b[ 0 ].Sterable )
        .ToList();

      foreach ( List<Brick> bigBrick in sterableBricks ) {
        List<Coords> coords = new List<Coords>();
        int minX = bigBrick[ 0 ].X;
        int maxX = bigBrick[ 0 ].X;
        int minY = bigBrick[ 0 ].Y;
        int maxY = bigBrick[ 0 ].Y;

        foreach ( Brick brick in bigBrick ) {
          if ( brick.X < minX )
            minX = brick.X;

          if ( brick.X > maxX )
            maxX = brick.X;

          if ( brick.Y < minY )
            minY = brick.Y;

          if ( brick.Y > maxY )
            maxY = brick.Y;
        }

        Coords pivot = new Coords( minX + (maxX - minX) / 2, minY + (maxY - minY) / 2 );
        bool canBeRotated = true;

        foreach ( Brick brick in bigBrick ) {
          int x = pivot.X + pivot.Y - brick.Y;
          int y = pivot.Y - pivot.X + brick.X;

          if ( !IsEmpty( x, y ) ) {
            canBeRotated = false;

            foreach ( Brick b in bigBrick )
              if ( b.X == x && b.Y == y ) {
                canBeRotated = true;
                break;
              }
          }

          if ( !canBeRotated )
            break;
        }

        if ( !canBeRotated )
          continue;

        foreach ( Brick brick in bigBrick ) {
          bool borderLeft = false;
          bool borderRight = false;
          bool borderTop = false;
          bool borderBottom = false;

          int x = pivot.X + pivot.Y - brick.Y;
          int y = pivot.Y - pivot.X + brick.X;

          if ( brick.BorderLeft )
            borderTop = true;

          if ( brick.BorderRight )
            borderBottom = true;

          if ( brick.BorderTop )
            borderRight = true;

          if ( brick.BorderBottom )
            borderLeft = true;

          if ( !coords.Exists( c => c.X == brick.X && c.Y == brick.Y ) )
            Bricks[ brick.Y, brick.X ] = null;

          coords.Add( new Coords( x, y ) );

          brick.BorderLeft = borderLeft;
          brick.BorderRight = borderRight;
          brick.BorderTop = borderTop;
          brick.BorderBottom = borderBottom;
          brick.X = x;
          brick.Y = y;

          Bricks[ brick.Y, brick.X ] = brick;
        }
      }
    }
    public void DestroyRows() {
      List<int> destryedRows = new List<int>();

      for ( int y = Height - 1;  y >= 0;  y-- ) {
        bool rowToDestroy = true;

        for ( int x = Width - 1;  x >= 0;  x-- ) {
          Brick brick = Get( x, y );

          if ( brick == null || brick.State == Brick.BrickState.Dynamic ) {
            rowToDestroy = false;
            break;
          }
        }

        if ( rowToDestroy )
          destryedRows.Add( y );
      }

      if ( destryedRows.Count == 0 )
        return;

      foreach ( int y in destryedRows )
        for ( int x = Width - 1;  x >= 0;  x-- ) {
          Bricks[ y, x ] = null;

          Brick up = Get( x, y - 1 );
          Brick down = Get( x, y + 1 );

          if ( up != null )
            up.BorderBottom = false;

          if ( down != null )
            down.BorderTop = false;
        }

      for ( int y = destryedRows[ 0 ] - 1;  y >= 0;  y-- )
        for ( int x = Width - 1;  x >= 0;  x-- ) {
          Brick brick = Get( x, y );

          if ( brick != null )
            brick.State = Brick.BrickState.Dynamic;
        }

      Score += (float) Math.Pow( 2, 1 + destryedRows.Count );
    }

    void _BuildBigBrick( List<Brick> bricks, int x, int y ) {
      if ( bricks.Exists( b => b.X == x && b.Y == y ) )
        return;

      Brick brick = Get( x, y );

      if ( brick == null )
        return;

      bricks.Add( brick );

      if ( brick.BorderLeft )
        _BuildBigBrick( bricks, x - 1, y );

      if ( brick.BorderRight )
        _BuildBigBrick( bricks, x + 1, y );

      if ( brick.BorderTop )
        _BuildBigBrick( bricks, x, y - 1 );

      if ( brick.BorderBottom )
        _BuildBigBrick( bricks, x, y + 1 );
    }
  }

  class Brick {
    public static Color [] colors = {
      Color.FromArgb( 255, 231, 101, 27 ),
      Color.FromArgb( 255, 239, 159, 28 ),
      Color.FromArgb( 255, 113, 44, 6 ),
      Color.FromArgb( 255, 234, 107, 72 ),
      Color.FromArgb( 255, 113, 169, 29 ),
      Color.FromArgb( 255, 247, 181, 127 ),
      Color.FromArgb( 255, 48, 181, 114 ),
      Color.FromArgb( 255, 255, 87, 13 ),
      Color.FromArgb( 255, 249, 244, 47 ),
      Color.FromArgb( 255, 0, 106, 178 ),
      Color.FromArgb( 255, 226, 36, 47 ),
      Color.FromArgb( 255, 60, 16, 123 ),
      Color.FromArgb( 255, 0, 75, 42 ),
      Color.FromArgb( 255, 153, 22, 60 ),
      Color.FromArgb( 255, 150, 129, 185 ),
      Color.FromArgb( 255, 244, 161, 187 ),
      Color.FromArgb( 255, 142, 206, 243 ),
      Color.FromArgb( 255, 255, 56, 252 )
    };
    public enum BrickState {
      Dynamic,
      Static
    }

    public static Random Random = new Random();

    public BrickState State = BrickState.Dynamic;
    public Color Color;

    public int X;
    public int Y;

    public string Owner;
    public bool BorderLeft;
    public bool BorderRight;
    public bool BorderTop;
    public bool BorderBottom;
    public bool Sterable = true;

    public Brick( string owner, int x, int y, bool borderLeft, bool borderRight, bool borderTop, bool borderBottom, Color color ) {
      this.Color = color == null  ?  Brick.colors[ Brick.Random.Next( Brick.colors.Length ) ]  :  color;
      this.Owner = owner;

      this.X = x;
      this.Y = y;

      this.BorderLeft = borderLeft;
      this.BorderRight = borderRight;
      this.BorderTop = borderTop;
      this.BorderBottom = borderBottom;
    }
  }
}