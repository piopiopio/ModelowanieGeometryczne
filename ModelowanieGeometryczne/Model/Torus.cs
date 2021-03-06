﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.RightsManagement;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using ModelowanieGeometryczne.Helpers;
using ModelowanieGeometryczne.ViewModel;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Platform;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;

namespace ModelowanieGeometryczne.Model
{
    public class Torus : ViewModelBase, IPatch
    {
        public bool Selected { get; set; }
        public static int torusID;
        public Vector4d TorusCenterPoint;
        public event PropertyChangedEventHandler RefreshTorus;
        public string Name { get; set; }
        #region Private fields
        private double _r;
        private double _R;
        private List<Vector4d> _verticesList = new List<Vector4d>();
        private List<Vector3d> _verticesListTransformed = new List<Vector3d>();
        private List<Tuple<int, int>> _relationsList = new List<Tuple<int, int>>();
        public int _division_fi;
        public int _division_teta;
        private object a;
        public double[] Center
        {
            get { return new double[] { TorusCenterPoint.X, TorusCenterPoint.Y, TorusCenterPoint.Z }; }
        }
        #endregion Private fields

        #region Public Properties
        public double r
        {
            get { return _r; }

            set
            {
                _r = value;
                OnPropertyChanged("r");
                generateParametrization();
                Refresh();
            }

        }

        public double R
        {
            get { return _R; }
            set
            {
                _R = value;
                OnPropertyChanged("R");
                generateParametrization();
                Refresh();
            }
        }

        public int Division_fi
        {
            get { return _division_fi; }
            set
            {
                _division_fi = value;
                OnPropertyChanged("Division_fi");
                generateParametrization();
                Refresh();
            }
        }

        public int Division_teta
        {
            get { return _division_teta; }
            set
            {
                _division_teta = value;
                OnPropertyChanged("Division_teta");
                generateParametrization();
                Refresh();
            }
        }

        #endregion Public Properties

        public Torus()
        {
            _r = 1;
            _R = 2;
            _division_fi = 20;
            _division_teta = 20;
            generateParametrization();
            torusID++;
            Name = "Torus " + torusID;


        }

        public Torus(Vector4d CenterPoint, double Torus_r, double Torus_R, int Torus_division_fi, int Torus_division_teta)
        {
            _r = Torus_r;
            _R = Torus_R;
            _division_fi = Torus_division_fi;
            _division_teta = Torus_division_teta;
            TorusCenterPoint = CenterPoint;
            generateParametrization();
            torusID++;
            Name = "Torus " + torusID;

        }

        #region Private Methods

        private void generateParametrization()
        {
            //TODO: Generate parametrization
            _verticesList.Clear();
            double fi, teta;
            double deltaFi = 2 * Math.PI / _division_fi;
            double deltaTeta = 2 * Math.PI / _division_teta;
            Vector4d temp = new Vector4d();
            int k = 0;

            _relationsList.Clear();
            _verticesList.Clear();
            for (int j = 0; j < _division_teta; j++)
            {

                for (int i = 0; i < _division_fi; i++)
                {
                    k = i + j * _division_fi;
                    fi = deltaFi * i;
                    teta = deltaTeta * j;
                    temp.X = (_R + _r * Math.Cos(fi)) * Math.Cos(teta) + TorusCenterPoint.X;
                    temp.Y = (_R + _r * Math.Cos(fi)) * Math.Sin(teta) + TorusCenterPoint.Y;
                    temp.Z = _r * Math.Sin(fi) + TorusCenterPoint.Z;
                    temp.W = 1;
                    _verticesList.Add(temp);

                    if (i > 0)
                    {
                        _relationsList.Add(new Tuple<int, int>(k - 1, k));
                    }

                    if (i == _division_fi - 1)
                    {
                        _relationsList.Add(new Tuple<int, int>(k, k - i));
                    }

                }

                if (j > 0)
                {
                    for (int i = 0; i < _division_fi; i++)
                    {
                        _relationsList.Add(new Tuple<int, int>(k - _division_fi - i, k - i));
                    }

                }

                if (j == Division_teta - 1)
                {
                    for (int i = 0; i < _division_fi; i++)
                    {
                        _relationsList.Add(new Tuple<int, int>(k - j * _division_fi - i, k - i));
                    }
                }
            }
        }

        private void Refresh()
        {
            if (RefreshTorus != null)
                RefreshTorus(this, new PropertyChangedEventArgs("RefreshScene"));
        }

        #endregion Private Methods

        #region Public Methods

        public void Draw(Matrix4d transformacja)
        {
            Matrix4d projekcja = MatrixProvider.ProjectionMatrix();
            GL.Begin(BeginMode.Lines);
            GL.Color3(1.0, 1.0, 1.0);

            foreach (var relations in _relationsList)
            {
                var vertex = transformacja.Multiply(_verticesList[relations.Item1]);
                var vertex2 = transformacja.Multiply(_verticesList[relations.Item2]);
                GL.Vertex2(projekcja.Multiply(vertex).X, projekcja.Multiply(vertex).Y);
                GL.Vertex2(projekcja.Multiply(vertex2).X, projekcja.Multiply(vertex2).Y);
                //}
                //GL.Vertex3(_verticesList[relations.Item1]);
                //GL.Vertex3(_verticesList[relations.Item2]);
            }
            GL.End();
        }

        public void DrawStereoscopy(Matrix4d transformacja)
        {
            const int renderWidth = 1440;
            const int renderHeight = 750;
            GL.Begin(BeginMode.Lines);
            GL.Color3(0.6, 0, 0);


            Matrix4d projekcja = MatrixProvider.RightProjectionMatrix();
            foreach (var relations in _relationsList)
            {
                var avertex = transformacja.Multiply(_verticesList[relations.Item1]);
                var avertex2 = transformacja.Multiply(_verticesList[relations.Item2]);
                var dx = projekcja.M13 * avertex.Z;
                var dx2 = projekcja.M13 * avertex2.Z;
                var vertex = projekcja.Multiply(avertex);
                var vertex2 = projekcja.Multiply(avertex2);
                GL.Vertex2(vertex.X, vertex.Y);
                GL.Vertex2(vertex2.X, vertex2.Y);

            }
            GL.End();

            Bitmap bmp1 = new Bitmap(renderWidth, renderHeight);
            System.Drawing.Imaging.BitmapData dat = bmp1.LockBits(new Rectangle(0, 0, renderWidth, renderHeight), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            GL.ReadPixels(0, 0, renderWidth, renderHeight, PixelFormat.Bgra, PixelType.UnsignedByte, dat.Scan0);
            bmp1.UnlockBits(dat);
            //bmp1.Save("D:\\ModelowanieGeometryczne\\a1.bmp", ImageFormat.Bmp);

            GL.Clear(ClearBufferMask.ColorBufferBit);


            GL.Begin(BeginMode.Lines);
            GL.Color3(0, 0, 0.9);


            projekcja = MatrixProvider.LeftProjectionMatrix();
            foreach (var relations in _relationsList)
            {

                var avertex = transformacja.Multiply(_verticesList[relations.Item1]);
                var avertex2 = transformacja.Multiply(_verticesList[relations.Item2]);
                var dx = projekcja.M13 * avertex.Z;
                var dx2 = projekcja.M13 * avertex2.Z;
                var vertex = projekcja.Multiply(avertex);
                var vertex2 = projekcja.Multiply(avertex2);
                GL.Vertex2(vertex.X, vertex.Y);
                GL.Vertex2(vertex2.X, vertex2.Y);

            }
            GL.End();

            Bitmap bmp2 = new Bitmap(renderWidth, renderHeight);
            System.Drawing.Imaging.BitmapData dat2 = bmp2.LockBits(new Rectangle(0, 0, renderWidth, renderHeight), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            GL.ReadPixels(0, 0, renderWidth, renderHeight, PixelFormat.Bgra, PixelType.UnsignedByte, dat2.Scan0);
            bmp2.UnlockBits(dat2);
            // bmp2.Save("D:\\ModelowanieGeometryczne\\a2.bmp", ImageFormat.Bmp);

            GL.Clear(ClearBufferMask.ColorBufferBit);

            Bitmap bmp3 = bmp2;

            Color temp, pixeltemp, result;

            ////Działa dla dowolnego koloru torusa 1 i torusa 2

            for (int i = 0; i < renderWidth; i++)
            {
                for (int j = 0; j < renderHeight; j++)
                {
                    temp = bmp1.GetPixel(i, j);
                    if (temp.R == 0 && temp.G == 0 && temp.B == 0)
                    {

                    }
                    else
                    {

                        pixeltemp = bmp3.GetPixel(i, j);
                        result = Color.FromArgb(Math.Min(temp.R + pixeltemp.R, 255), Math.Min(temp.G + pixeltemp.G, 255), Math.Min(temp.B + pixeltemp.B, 255));
                        bmp3.SetPixel(i, j, result);
                    }
                }
            }

            //Writable bitmap
            bmp3.Save("D:\\ModelowanieGeometryczne\\a3.bmp", ImageFormat.Bmp);



            //Color k1 = Color.FromArgb(87, 0, 0);
            //Color k2 = Color.FromArgb(87, 0, 173);
            //for (int i = 0; i < RenderWidth; i++)
            //{
            //    for (int j = 0; j < RenderHeight; j++)
            //    {


            //        if (bmp1.GetPixel(i, j).R == 0)
            //        {

            //        }
            //        else if (bmp3.GetPixel(i, j).B == 0)
            //        {

            //            //Blending colors overlapped lines.

            //            bmp3.SetPixel(i, j, k1);
            //        }
            //        else
            //        {

            //            bmp3.SetPixel(i, j, k2);
            //        }

            //    }
            //}
            //bmp3.Save("D:\\ModelowanieGeometryczne\\a3.bmp", ImageFormat.Bmp);


            dat2 = bmp2.LockBits(new Rectangle(0, 0, renderWidth, renderHeight), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            GL.DrawPixels(renderWidth, renderHeight, PixelFormat.Bgra, PixelType.UnsignedByte, dat2.Scan0);
            bmp2.UnlockBits(dat2);


        }

        public Point GetPoint(double u, double v)
        {
            //double[] coord = GetPatchNumber(u, v);
            //var _pointsToDrawSinglePatch = Copy4x4PieceOfPointsCollecion(3 * (int)coord[0], 3 * (int)coord[1]);
            //return MatrixProvider.Multiply(CalculateB(coord[2]), _pointsToDrawSinglePatch, CalculateB(coord[3]));
            if (u > 1)
            {
               // u -= 1;
            }

            if (u < 0)
            {
                //u += 1;
            }

            if (v > 1)
            {
                //v -= 1;
            }

            if (v < 0)
            {
              //  v += 1;
            }
            double fi = 2 * Math.PI * u;
            double teta = 2 * Math.PI * v;
            Vector4d temp = new Vector4d();
            temp.X = (_R + _r * Math.Cos(fi)) * Math.Cos(teta) + TorusCenterPoint.X;
            temp.Y = (_R + _r * Math.Cos(fi)) * Math.Sin(teta) + TorusCenterPoint.Y;
            temp.Z = _r * Math.Sin(fi) + TorusCenterPoint.Z;

            return new Point(temp.X, temp.Y, temp.Z);
        }



        public Point GetPointDerivativeU(double u, double v)
        {
            double fi = 2 * Math.PI * u;
            double teta = 2 * Math.PI * v;

            double X = -r * Math.Sin(fi) * Math.Cos(teta) * 2 * Math.PI;
            double Y = -r * Math.Sin(fi) * Math.Sin(teta) * 2 * Math.PI;
            double Z = r * Math.Cos(fi) * 2 * Math.PI;

            return new Point(X, Y, Z);
        }

        public Point GetPointDerivativeV(double u, double v)
        {
            double fi = 2 * Math.PI * u;
            double teta = 2 * Math.PI * v;

            double X = -(R + r * Math.Cos(fi)) * Math.Sin(teta) * 2 * Math.PI;
            double Y = (R + r * Math.Cos(fi)) * Math.Cos(teta) * 2 * Math.PI;
            double Z = 0;
            return new Point(X, Y, Z);
        }


        #endregion Public Methods
    }

}

