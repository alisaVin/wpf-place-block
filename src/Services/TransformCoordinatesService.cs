using System;
using System.Collections.Generic;
using Teigha.Geometry;

namespace place_block_wpf_ares.src.Services
{
    public static class TransformCoordinatesService
    {
        public static List<Point3d> Rotate(List<Point3d> originalCoords)
        {
            List<Point3d> rotatedCoords = new List<Point3d>();

            // -90° Rotation im Uhrzeigersinn: (x,y) -> (-y, x)
            foreach (Point3d coord in originalCoords)
            {
                double newX = -coord.Y;
                double newY = coord.X;
                rotatedCoords.Add(new Point3d(newX, newY, 0));
            }
            // Finden minimalen X-Wert für Offset-Berechnung
            double minX = double.MaxValue;
            // Offset der X berechnen
            double xOffset = Math.Abs(minX);

            List<Point3d> finalCoords = new List<Point3d>();
            foreach (Point3d coord in rotatedCoords)
            {
                finalCoords.Add(new Point3d(coord.X + xOffset, coord.Y, 0));
            }
            return finalCoords;
        }
    }
}
