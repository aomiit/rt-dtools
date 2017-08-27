﻿using DicomPanel.Core.Geometry;
using DicomPanel.Core.Render;
using DicomPanel.Core.Utilities.RTMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DicomPanel.Core.Render
{
    /// The camera represents a plane inside a 3D grid with ColDir and RowDir running on the plane. The posiiton is supposed to represent the "center" of the collimated view plane.
    /// </summary>
    public partial class Camera
    {
        public Point3d Position { get; set; }
        public Point3d ColDir { get; private set; }
        private double colDirLength { get; set; }
        public Point3d RowDir { get; private set; }
        private double rowDirLength { get; set; }
        public Point3d Normal { get; private set; }

        private Point3d _screenCoordsCache { get; set; }
        private Point3d _worldCoordsCache { get; set; }

        /// <summary>
        /// The camera's x and y FOV in mm
        /// </summary>
        private Point2d FOV { get; set; }
       
        public double Scale { get; set; }
        public double MMPerPixel { get; set; }

        public bool IsAxial { get { return (Normal.Z != 0 && Normal.X == 0 && Normal.Y == 0); } }

        public Camera()
        {
            ColDir = new Point3d();
            RowDir = new Point3d();
            Normal = new Point3d();
            Position = new Point3d();
            _screenCoordsCache = new Point3d();
            _worldCoordsCache = new Point3d();
            _screenToWorldMatrix = new Matrix3d();
            FOV = new Point2d(0, 0);
            Scale = 1;
            MMPerPixel = 1;
            SetAxial();
        }
        
        private void onUpdateView()
        {
            Normal = ColDir.Cross(RowDir);
            createScreenToWorldMatrix();
            cacheWorldToScreenVariables();
            colDirLength = ColDir.Length();
            rowDirLength = RowDir.Length();
        }

        public void SetFOV(double width, double height)
        {
            FOV.X = width;
            FOV.Y = height;
            onUpdateView();
        }

        public Point2d GetFOV()
        {
            return FOV;
        }

        /// <summary>
        /// Moves the camera by the specified amount
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public void Move(double x, double y, double z)
        {
            MoveTo(Position.X + x, Position.Y + y, Position.Z + z);
        }

        /// <summary>
        /// Moves the camera by the specified amount
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public void Move(Point3d amount)
        {
            Move(amount.X, amount.Y, amount.Z);
        }

        public void Zoom(double amount)
        {
            Scale *= amount;
            this.onUpdateView();
        }

        /// <summary>
        /// Move to a position in the patient coordinate system
        /// </summary>
        /// <param name="x">The x position (mm)</param>
        /// <param name="y">The y position (mm)</param>
        /// <param name="z">The z position (mm)</param>
        public void MoveTo(double x, double y, double z)
        {
            Position.X = x;
            Position.Y = y;
            Position.Z = z;
            this.onUpdateView();
        }

        public void SetDirections(double colDirx, double colDiry, double colDirz,
            double rowDirx, double rowDiry, double rowDirz)
        {
            ColDir.X = colDirx;
            ColDir.Y = colDiry;
            ColDir.Z = colDirz;
            RowDir.X = rowDirx;
            RowDir.Y = rowDiry;
            RowDir.Z = rowDirz;
            this.onUpdateView();
        }

        public void SetAxial()
        {
            SetDirections(1, 0, 0, 0, 1, 0);
        }
        public void SetSagittal()
        {
            SetDirections(0, -1, 0, 0, 0, -1);
        }
        public void SetCoronal()
        {
            SetDirections(1, 0, 0, 0, 0, - 1);
        }

        /// <summary>
        /// Scrolls through patient slices in the direciton of the camera normal vector
        /// </summary>
        /// <param name="direction">Positive or negative value indicating direction with respect to the normal vector</param>
        public void Scroll(double direction, double amount)
        {
            if (direction == 0)
                return;

            Point3d movDir = new Point3d();
            Normal.CopyTo(movDir);
            movDir *= (direction / Math.Abs(direction));
            Position += movDir * amount;

            this.onUpdateView();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="thetax">Angle around the x axis in degrees</param>
        /// <param name="thetay">Angle around the y axis in degrees</param>
        /// <param name="thetaz">Angle around the z axis in degrees</param>
        public void Rotate(double thetax, double thetay, double thetaz)
        {
            double r_thetax = thetax * Math.PI / 180;
            double r_thetay = thetay * Math.PI / 180;
            double r_thetaz = thetaz * Math.PI / 180;
            var Rx = Matrix3d.GetRotationX(thetax);
            var Ry = Matrix3d.GetRotationY(thetay);
            var Rz = Matrix3d.GetRotationZ(thetaz);
            ColDir = Rx * Ry * Rz * ColDir;
            RowDir = Rx * Ry * Rz * RowDir;
            colDirLength = ColDir.Length();
            rowDirLength = RowDir.Length();
            Normal = ColDir.Cross(RowDir);

            this.onUpdateView();
        }


    }
}