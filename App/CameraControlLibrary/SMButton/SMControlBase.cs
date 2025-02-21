﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace CameraControlLibrary.CameraHIK
{
    [Designer("System.Windows.Forms.Design.ParentControlDesigner, System.Design", typeof(System.ComponentModel.Design.IDesigner))]
    public partial class SMControlBase : UserControl,IContainerControl
    {
        private bool _isRadius = false;
 
        private int _cornerRadius = 24;
 
        private bool _isShowRect = false;
 
        private Color _rectColor = Color.FromArgb(220, 220, 220);

        private int _rectWidth = 1;
 
        private Color _fillColor = Color.Transparent;

        [ Description("是否圆角"), Category("SmoreControl")]
        public bool IsRadius
        {
            get
            {
                return this._isRadius;
            }
            set
            {
                this._isRadius = value;
            }
        }

        [Description("圆角角度"), Category("SmoreControl")]
        public int ConerRadius
        {
            get
            {
                return this._cornerRadius;
            }
            set
            {
                this._cornerRadius = value;
            }
        }

        [Description("是否显示边框"), Category("SmoreControl")]
        public bool IsShowRect
        {
            get
            {
                return this._isShowRect;
            }
            set
            {
                this._isShowRect = value;
            }
        }

        [Description("边框颜色"), Category("SmoreControl")]
        public Color RectColor
        {
            get
            {
                return this._rectColor;
            }
            set
            {
                this._rectColor = value;
                this.Refresh();
            }
        }

        [Description("边框宽度"), Category("SmoreControl")]
        public int RectWidth
        {
            get
            {
                return this._rectWidth;
            }
            set
            {
                this._rectWidth = value;
            }
        }

        [Description("当使用边框时填充颜色，当值为背景色或透明色或空值则不填充"), Category("SmoreControl")]
        public Color FillColor
        {
            get
            {
                return this._fillColor;
            }
            set
            {
                this._fillColor = value;
            }
        }
        public SMControlBase()
        {
            InitializeComponent();
            base.SetStyle(ControlStyles.UserPaint, true);
            base.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            base.SetStyle(ControlStyles.DoubleBuffer, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
           if (this.Visible)
           {
                if (this._isRadius)
                    {
                        this.SetWindowRegion();
                    }
                if (this._isShowRect)
                {
                    Color rectColor = this._rectColor;
                    Pen pen = new Pen(rectColor, (float)this._rectWidth);
                    Rectangle clientRectangle = base.ClientRectangle;
                    GraphicsPath graphicsPath = new GraphicsPath();
                    graphicsPath.AddArc(0, 0, _cornerRadius, _cornerRadius, 180f, 90f);
                    graphicsPath.AddArc(clientRectangle.Width - _cornerRadius - 1, 0, _cornerRadius, _cornerRadius, 270f, 90f);
                    graphicsPath.AddArc(clientRectangle.Width - _cornerRadius - 1, clientRectangle.Height - _cornerRadius - 1, _cornerRadius, _cornerRadius, 0f, 90f);
                    graphicsPath.AddArc(0, clientRectangle.Height - _cornerRadius - 1, _cornerRadius, _cornerRadius, 90f, 90f);
                    graphicsPath.CloseFigure();
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    if (_fillColor != Color.Empty && _fillColor != Color.Transparent && _fillColor != this.BackColor)
                                e.Graphics.FillPath(new SolidBrush(this._fillColor), graphicsPath);
                        e.Graphics.DrawPath(pen, graphicsPath);
                }
           }
           base.OnPaint(e);
        }

         private void SetWindowRegion()
         {
            GraphicsPath path = new GraphicsPath();
            Rectangle rect = new Rectangle(-1, -1, base.Width + 1, base.Height);
            path = this.GetRoundedRectPath(rect, this._cornerRadius);
            base.Region = new Region(path);
         }

        private GraphicsPath GetRoundedRectPath(Rectangle rect, int radius)
        {
             Rectangle rect2 = new Rectangle(rect.Location, new Size(radius, radius));
             GraphicsPath graphicsPath = new GraphicsPath();
             graphicsPath.AddArc(rect2, 180f, 90f);//左上角
             rect2.X = rect.Right - radius;
             graphicsPath.AddArc(rect2, 270f, 90f);//右上角
             rect2.Y = rect.Bottom - radius;
             rect2.Width += 1;
             rect2.Height += 1;
             graphicsPath.AddArc(rect2, 360f, 90f);//右下角           
             rect2.X = rect.Left;
             graphicsPath.AddArc(rect2, 90f, 90f);//左下角
             graphicsPath.CloseFigure();
             return graphicsPath;
         }

         protected override void WndProc(ref Message m)
         {
            if (m.Msg != 20)
            {
                 base.WndProc(ref m);
            }
         }
    }
}
