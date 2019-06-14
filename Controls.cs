using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Tetris {
  class Box : Panel {
    Color color;
    Pen pen;
    Size measuredFont;
    int borderWidth = 4;
    string label = "";

    public int DrawingWidth {
      get => Width - 2 * (borderWidth + 1);
    }
    public int DrawingHeight {
      get => Height - measuredFont.Height - borderWidth - 2;
    }

    public Box( Color color, string label, int left, int top, int width=200, int height=150 ) {
      if ( color == null )
        this.color = Color.FromArgb( 40, 255, 255, 255 );
      else
        this.color = color;

      this.label = label;

      this.Left = left;
      this.Top = top;
      this.Width = width;
      this.Height = height;

      this.pen = new Pen( this.color );
      this.pen.Width = borderWidth;

      this.Font = new Font( FontFamily.GenericMonospace, 10, FontStyle.Bold );

      measuredFont = TextRenderer.MeasureText( label, this.Font );
    }

    public string Label {
      get => label;
      set {
        label = value;
        measuredFont = TextRenderer.MeasureText( value, this.Font );

        Invalidate();
      }
    }

    public void UpdateSize( int width, int height ) {
      Width = width + 2 * (borderWidth + 1);
      Height = height + measuredFont.Height + borderWidth + 2;
    }
    public void Clear() {
      int top = measuredFont.Height + 1;

      this.CreateGraphics().FillRectangle(
        new SolidBrush( this.BackColor ),
        new Rectangle( borderWidth, top, this.Width - borderWidth * 2, this.Height - top - borderWidth )
      );
    }
    public void FillRectangle( Color color, int x, int y, int width, int height ) {
      if ( x < 0 )
        x = 0;

      if ( y < 0 )
        y = 0;

      if ( x > DrawingWidth )
        x = DrawingWidth;

      if ( y > DrawingHeight )
        y = DrawingHeight;

      if ( x + width > DrawingWidth )
        width = DrawingWidth - x;

      if ( y + height > DrawingHeight )
        height = DrawingHeight - y;

      this.CreateGraphics().FillRectangle(
        new SolidBrush( color ),
        new Rectangle( borderWidth + 1 + x, measuredFont.Height + 1 + y, width, height )
      );
    }

    protected override void OnPaint( PaintEventArgs e ) {
      base.OnPaint( e );

      SolidBrush brushText = new SolidBrush( this.color );

      int left = (this.Width - measuredFont.Width) / 2;
      int top = measuredFont.Height / 2 + 1;
      int halfBorderWidth = (int) pen.Width / 2;

      Point [] points = {
        new Point( left, top ),
        new Point( halfBorderWidth, top ),
        new Point( halfBorderWidth, this.Height - halfBorderWidth ),
        new Point( this.Width - halfBorderWidth, this.Height - halfBorderWidth ),
        new Point( this.Width - halfBorderWidth, top ),
        new Point( left + measuredFont.Width, top ),
      };

      e.Graphics.DrawLines( pen, points );
      e.Graphics.DrawString( label, this.Font, brushText, new PointF( left, 0 ) );
    }
  }

  class TextArea : Box {
    Color _TextColor;
    string _Text;

    public TextArea(
      Color color,
      Color textColor,
      string text,
      string label,
      int left,
      int top,
      int width = 200,
      int height = 150
    ) : base( color, label, left, top, width, height ) {
      _TextColor = textColor;
      _Text = text;

      Paint += new PaintEventHandler( (object sender, PaintEventArgs e) => {
        e.Graphics.DrawString(
          _Text,
          new Font( FontFamily.GenericMonospace, 8 ),
          new SolidBrush( _TextColor ),
          new PointF( 10, 10 )
        );
      } );
    }

    public new string Text {
      get => _Text;
      set {
        _Text = value;

        Invalidate();
      }
    }
  }

  class DataArea : Box {
    public struct Item {
      public string Name;
      public string Value;
    };

    public List<Item> Items;

    protected Color _TextColor;

    public DataArea(
      List<Item> items,
      Color color,
      Color textColor,
      string label,
      int left,
      int top,
      int width = 200,
      int height = 150
    ) : base( color, label, left, top, width, height ) {
      Items = items;

      _TextColor = textColor;

      _Update();
    }

    public void AddItem( string name, string value="" ) {
      Items.Add( new Item() { Name=name, Value=value } );

      _Update();
    }
    public void RemoveItem( string name ) {
      int index = Items.FindIndex( i => i.Name == name );

      if ( index != -1 )
        Items.RemoveAt( index );

      _Update();
    }
    public void ClearItems() {
      Items.Clear();
    }
    public void ChangeItemValue( string name, string value ) {
      int index = Items.FindIndex( i => i.Name == name );

      if ( index != -1 )
        Items[ index ] = new Item() { Name=name, Value=value };

      _Update();
    }

    protected virtual void _Update() {
      string text = "";

      foreach ( Item item in Items )
        text += item.Name + ": " + item.Value + "\n";

      Clear();
      CreateGraphics().DrawString(
        text,
        new Font( FontFamily.GenericMonospace, 8 ),
        new SolidBrush( _TextColor ),
        new PointF( 20, 25 )
      );
    }
  }

  class Scoreboard : DataArea {

    public Scoreboard(
      List<Item> items,
      Color color,
      Color textColor,
      string label,
      int left,
      int top,
      int width = 200,
      int height = 150
    ) : base( items, color, textColor, label, left, top, width, height ) {}

    protected override void _Update() {
      string text = "";
      Items.Sort( (a, b) => int.Parse( b.Value ).CompareTo( int.Parse( a.Value ) ) );

      foreach ( Item item in Items )
        text += item.Name + ": " + item.Value + "\n";

      Clear();
      CreateGraphics().DrawString(
        text,
        new Font( FontFamily.GenericMonospace, 8 ),
        new SolidBrush( _TextColor ),
        new PointF( 20, 25 )
      );
    }

  }

  class Btn : Panel {
	  public delegate void OnClickEvent();

    protected OnClickEvent _OnClick;
    string _Text = "";
    Color _TxtColor;
    Pen _Pen;

    public Btn(
      OnClickEvent onClick,
      Color txtColor,
      Color bgrColor,
      Color hoverBgrColor,
      string label,
      int left=0,
      int top=0,
      int width=0,
      int height=20
    ) {
      _TxtColor = txtColor;
      _Text = label;
      _Pen = new Pen( txtColor );
      _Pen.Width = 3;

      _OnClick = onClick;
      Click += new EventHandler( (object obj, EventArgs e) => Onclick() );
      MouseEnter += new EventHandler( (object sender, EventArgs e) => {
        Cursor = Cursors.Hand;
        BackColor = hoverBgrColor;
      } );
      MouseLeave += new EventHandler( (object sender, EventArgs e) => {
        BackColor = bgrColor;
      } );

      BackColor = bgrColor;
      Font = new Font( FontFamily.GenericMonospace, 8 );
      Height = height;
      Left = left;
      Top = top;

      var size = TextRenderer.MeasureText( Text, Font );

      if ( size.Width + 10 > width )
        Width = size.Width + 10;
      else
        Width = width;
    }
    public new string Text {
      get => _Text;
      set {
        _Text = value;

        var size = TextRenderer.MeasureText( _Text, Font );

        if ( size.Width + 10 > Width )
          Width = size.Width + 10;

        Invalidate();
      }
    }
    protected virtual void Onclick() {
      _OnClick();
    }
    protected override void OnPaint( PaintEventArgs e ) {
      base.OnPaint( e );

      SolidBrush txtBrush = new SolidBrush( _TxtColor );

      var size = TextRenderer.MeasureText( this.Text, this.Font );

      int left = (Width - size.Width) / 2;
      int top = this.Height / 2 - size.Height / 2;

      e.Graphics.DrawString( _Text, this.Font, txtBrush, new PointF( left, top ) );
    }
  }

  class BtnRadio : Btn {
    bool _State;
    string _Text;

    public BtnRadio(
      OnClickEvent onClick,
      bool state,
      Color txtColor,
      Color bgrColor,
      Color hoverBgrColor,
      string label,
      int left=0,
      int top=0,
      int width=0,
      int height=20
    ) : base( onClick, txtColor, bgrColor, hoverBgrColor, label, left, top, width, height ) {
      _State = state;
      _Text = label;
      Text = (state  ?  "[ on ]"  :  "[ off ]") + " " + label;
    }

    protected override void Onclick() {
      _State = !_State;

      if ( _State )
        _OnClick();

      Text = (_State  ?  "[ on ]"  :  "[ off ]") + " " + _Text;

      Invalidate();
    }
  }

  class Input : Panel {
    TextBox _Input = new TextBox();
    Color _BgrColor;
    Color _TextColor;
    string _Label;

    override public string Text {
      get => _Input.Text;
      set => _Input.Text = value;
    }

    public Input( Color backgroundColor, Color color, string label, int left, int top, int width=50, int height=15 ) {
      Width = width;
      Height = height;
      Left = left;
      Top = top;

      _BgrColor = backgroundColor;
      _TextColor = color;
      _Label = label;

      _Input.Top = Height / 2 - 1;
      _Input.Width = Width;
      _Input.Height = Height / 2 - 1;
      _Input.Font = new Font( FontFamily.GenericMonospace, _Input.Font.Size, FontStyle.Bold );

      _Input.ForeColor = color;
      _Input.BackColor = backgroundColor;
      _Input.BorderStyle = BorderStyle.None;

      Controls.Add( _Input );
    }

    protected override void OnPaint( PaintEventArgs e ) {
      Graphics g = e.Graphics;

      Color labelColor = Color.FromArgb(
        (int) (_TextColor.A * .9),
        (int) (_TextColor.R * .9),
        (int) (_TextColor.G * .9),
        (int) (_TextColor.B * .9)
      );

      g.DrawString(
        _Label,
        new Font( FontFamily.GenericMonospace, (float) (_Input.Font.Size * .9), FontStyle.Bold ),
        new SolidBrush( labelColor ),
        new PointF( 0, 0 )
      );

      g.DrawLine( new Pen( _TextColor ), new Point( 0, Height - 1 ), new Point( Width, Height - 1 ) );
    }
  }

  class Menu : Panel {
    public enum BorderPos {
      // Left,
      // Rigth,
      Top,
      Bottom
    }

    public bool Dragging { get; private set; }
    public string Title {
      get => _title;
      set {
        _title = value;

        Invalidate();
      }
    }

    List<Btn> _LeftButtons = new List<Btn>();
    List<Btn> _RightButtons = new List<Btn>();
    BorderPos _BorderPos;
    Point _PointClicket;
    Color _titleColor;
    string _title = "";

    public Menu( Form form, int x, int y, int width, int height, BorderPos borderPos ) {
      Left = x;
      Top = y;
      Width = width;
      Height = height;
      Font = new Font( FontFamily.GenericMonospace, 10, FontStyle.Bold );

      _BorderPos = borderPos;

      MouseUp += new MouseEventHandler( (object sender, MouseEventArgs e) => Dragging = false );
      MouseDown += new MouseEventHandler( (object sender, MouseEventArgs e) => {
        if ( e.Button == MouseButtons.Left ) {
          Dragging = true;
          _PointClicket = new Point( e.X, e.Y );
        }
        else
          Dragging = false;
      } );
      MouseMove += new MouseEventHandler( (object sender, MouseEventArgs e) => {
        if ( Dragging ){
          Point pointMoveTo = this.PointToScreen( new Point( e.X, e.Y ) );

          pointMoveTo.Offset( -_PointClicket.X, -_PointClicket.Y );

          form.Location = pointMoveTo;
        }
      } );
    }

    public void AddTitle( Color color, string title ) {
      _titleColor = color;
      Title = title;
    }

    public void AddBtnLeft( Btn.OnClickEvent onClick, Color txtColor, Color bgrColor, Color hoverBgrColor, string label ) {
      int top = 0;

      if ( _BorderPos == BorderPos.Bottom )
        top = 0;
      else if ( _BorderPos == BorderPos.Top )
        top = 2;

      Btn button = new Btn( onClick, txtColor, bgrColor, hoverBgrColor, label, 0, top, Height, Height - 2 );

      _LeftButtons.Add( button );
      Controls.Add( button );

      Invalidate();
    }
    public void AddBtnRight( Btn.OnClickEvent onClick, Color txtColor, Color bgrColor, Color hoverBgrColor, string label ) {
      int top = 0;

      if ( _BorderPos == BorderPos.Bottom )
        top = 0;
      else if ( _BorderPos == BorderPos.Top )
        top = 2;

      Btn button = new Btn( onClick, txtColor, bgrColor, hoverBgrColor, label, 0, top, Height, Height - 2 );

      _RightButtons.Add( button );
      Controls.Add( button );

      Invalidate();
    }

    public void AddRadioRight( Btn.OnClickEvent onClick, bool state, Color txtColor, Color bgrColor, Color hoverBgrColor, string label ) {
      int top = 0;

      if ( _BorderPos == BorderPos.Bottom )
        top = 0;
      else if ( _BorderPos == BorderPos.Top )
        top = 2;

      BtnRadio button = new BtnRadio( onClick, state, txtColor, bgrColor, hoverBgrColor, label, 0, top, Height, Height - 2 );

      _RightButtons.Add( button );
      Controls.Add( button );

      Invalidate();
    }

    protected override void OnPaint( PaintEventArgs e ) {
      base.OnPaint( e );

      int left = 0;

      foreach ( Btn button in _LeftButtons ) {
        button.Left = left;
        left += button.Width;
      }

      left = Width;

      foreach ( Btn button in _RightButtons ) {
        left -= button.Width;
        button.Left = left;
      }

      Pen brightPen = new Pen( Color.FromArgb( 30, 255, 255, 255 ) );
      Pen darkPen = new Pen( Color.FromArgb( 255, 0, 0, 0 ) );

      SolidBrush bgr = new SolidBrush( Color.FromArgb( 10, 200, 200, 255 ) );
      Rectangle rect = new Rectangle( 0, 0, Width, Height );

      e.Graphics.FillRectangle( bgr, rect );

      if ( _BorderPos == BorderPos.Bottom ) {
        e.Graphics.DrawLine( darkPen, new Point( 0, Height - 2 ), new Point( Width, Height - 2 ) );
        e.Graphics.DrawLine( brightPen, new Point( 0, Height - 1 ), new Point( Width, Height - 1 ) );
      }
      else if ( _BorderPos == BorderPos.Top ) {
        e.Graphics.DrawLine( darkPen, new Point( 0, 0 ), new Point( Width, 0 ) );
        e.Graphics.DrawLine( brightPen, new Point( 0, 1 ), new Point( Width, 1 ) );
      }


      if ( _title != "" ) {
        Brush titleBrush = new SolidBrush( _titleColor );
        Size title = TextRenderer.MeasureText( _title, Font );

        e.Graphics.DrawString( _title, Font, titleBrush, new Point(
          Width / 2 - title.Width / 2, Height / 2 - title.Height / 2
        ) );
      }
    }
  }
}
