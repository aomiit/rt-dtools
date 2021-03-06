﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RT.Core.Utilities.RTMath;
using RT.Core.Dose;
using RT.Core.Planning;

namespace RT.Core.Geometry
{
    public abstract class VoxelDataStructureBase
    {
        public Range XRange { get; set; }
        public Range YRange { get; set; }
        public Range ZRange { get; set; }
        public Voxel MaxVoxel { get; set; }
        public Voxel MinVoxel { get; set; }
        private Point3d positionCache;
        private Voxel voxelCache;

        public double RxDose { get; set; }
        public double NormalisationPercent { get; set; } = 100;
        public NormalisationType NormalisationType { get; set; } = NormalisationType.Relative;
        public RelativeNormalisationOption RelativeNormalisationOption { get; set; }
        public PointOfInterest NormalisationPOI { get; set; }

        /// <summary>
        /// Converts actual value to value of type Unit
        /// </summary>
        public float Scaling { get; set; } = 1;

        /// <summary>
        /// The physical unit
        /// </summary>
        public Unit ValueUnit { get; set; }

        public string Name { get; set; }

        public VoxelDataStructureBase()
        {
            XRange = new Range();
            YRange = new Range();
            ZRange = new Range();
            positionCache = new Point3d();
            voxelCache = new Voxel();
            MaxVoxel = new Voxel() { Value = float.MinValue };
            MinVoxel = new Voxel() { Value = float.MaxValue };
        }

        public Voxel Interpolate(double x, double y, double z)
        {
            Voxel voxel = new Voxel();
            Interpolate(x, y, z, voxel);
            return voxel;
        }

        public Voxel Interpolate(Point3d position)
        {
            Interpolate(position.X, position.Y, position.Z, voxelCache);
            return voxelCache;
        }

        public void Interpolate(double x, double y, double z, Voxel voxel)
        {
            positionCache.X = x;
            positionCache.Y = y;
            positionCache.Z = z;
            Interpolate(positionCache, voxel);
        }

        public bool ContainsPoint(double x, double y, double z)
        {
            return XRange.Contains(x) && YRange.Contains(y) && ZRange.Contains(z);
        }

        public bool ContainsPoint(Point3d point)
        {
            return ContainsPoint(point.X, point.Y, point.Z);
        }

        public float this[Point3d point]
        {
            get
            {
                return Interpolate(point).Value;
            }
        }

        public abstract void Interpolate(Point3d position, Voxel voxel);

        public float GetNormalisationAmount()
        {
            if (NormalisationType == NormalisationType.Absolute)
                return 1;

            if (NormalisationType == NormalisationType.Relative)
            {
                float value = 1.0f;

                if (RelativeNormalisationOption == RelativeNormalisationOption.Rx)
                {
                    value = 1.0f;
                }                
                else if (RelativeNormalisationOption == RelativeNormalisationOption.Max)
                {
                    value = MaxVoxel.Value;
                }                    
                else if (RelativeNormalisationOption == RelativeNormalisationOption.POI && NormalisationPOI != null)
                {
                    value = this.Interpolate(NormalisationPOI.Position).Value;
                }
                return  value * Scaling * ((float)NormalisationPercent / (100* 100));
            }
            return 1;
        }
    }
}
