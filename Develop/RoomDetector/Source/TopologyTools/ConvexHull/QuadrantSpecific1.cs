﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TopologyTools.Utils;

namespace TopologyTools.ConvexHull
{
	public class QuadrantSpecific1<T> : Quadrant<T>
	{
		// ************************************************************************
        public QuadrantSpecific1(IEnumerable<IEnumerable<T>> listOfListOfPoint, int initialResultGuessSize,
            Func<T, double> xGet, Func<T, double> yGet, Func<double, double, T> pointConstructor, Func<T, T, bool> pointEqual)
            : base(listOfListOfPoint, initialResultGuessSize, xGet, yGet, pointConstructor, pointEqual)
		{
		}

		// ******************************************************************
		protected override void SetQuadrantLimits()
		{
			T firstPoint = this._listOfListOfPoint.First().First();

			double rightX = _xGet(firstPoint);
			double rightY = _yGet(firstPoint);

			double topX = rightX;
			double topY = rightY;

			foreach (var enumOfPoints in _listOfListOfPoint)
			{
				foreach (var point in enumOfPoints)
				{
					if (_xGet(point).LargerOrEqualWithTol(rightX))
					{
						if (_xGet(point).EqualsWithTol( rightX))
						{
							if (_yGet(point).LargerWithTol( rightY))
							{
								rightY = _yGet(point);
							}
						}
						else
						{
							rightX = _xGet(point);
							rightY = _yGet(point);
						}
					}

					if (_yGet(point).LargerOrEqualWithTol(topY))
					{
						if (_yGet(point).EqualsWithTol(topY))
						{
							if (_xGet(point).LargerWithTol( topX))
							{
								topX = _xGet(point);
							}
						}
						else
						{
							topX = _xGet(point);
							topY = _yGet(point);
						}
					}
				}
			}

            FirstPoint = new PointInfo<T>(_pointConstructor(rightX, rightY), _xGet, _yGet, _pointEqual);
            LastPoint = new PointInfo<T>(_pointConstructor(topX, topY), _xGet, _yGet, _pointEqual);
            RootPoint = _pointConstructor(topX, rightY);
		}

		// ******************************************************************
		protected override double GetLastPointSlopeToNext()
		{
			return 0;
		}

		// ******************************************************************
		public override bool IsValueCannotBeConvexValueToAnotherOne(double x, double y, double xRef, double yRef)
		{
			if (x.SmallerWithTol(xRef))
			{
				if (y.SmallerOrEqualWithTol(yRef))
				{
					return true;
				}
			}
			else if (y.SmallerWithTol(yRef))
			{
				if (x.SmallerOrEqualWithTol( xRef))
				{
					return true;
				}
			}

			return false;
		}

		// ******************************************************************
		protected override bool IsGoodQuadrantForPoint(T pt)
		{
			if (_xGet(pt).LargerWithTol( _xGet(this.RootPoint)) && _yGet(pt).LargerWithTol( _yGet(this.RootPoint)))
			{
				return true;
			}

			return false;
		}

		// ******************************************************************
		protected override void TryAdd(double x, double y)
		{
			int indexLow = 0;
			int indexMax = HullPoints.Count - 1;
			int indexHi = indexMax;

			while (indexLow != indexHi - 1)
			{
				int index = ((indexHi - indexLow) >> 1) + indexLow;

				if (IsValueCannotBeConvexValueToAnotherOne(x, y, HullPoints[index].X, HullPoints[index].Y))
				{
					return;
				}
				
				if (x.LargerWithTol( HullPoints[index].X))
				{
					indexHi = index;
					continue;
				}

				if (x.SmallerWithTol(HullPoints[index].X))
				{
					indexLow = index;
					continue;
				}

				if (x.EqualsWithTol( HullPoints[index].X))
				{
					indexLow = index;
					indexHi = index + 1;
				}

				break;
			}

			PointInfo<T> ptiBefore = HullPoints[indexLow];

			if (y.SmallerOrEqualWithTol( ptiBefore.Y))
			{
				return; // Eliminated without slope calc
			}

			PointInfo<T> ptiAfter = HullPoints[indexHi];

			double slopeToNext = Geometry.CalcSlope(x, y, ptiAfter.X, ptiAfter.Y);

			if (slopeToNext.LargerWithTol( ptiBefore.SlopeToNext))
			{
				// We keep it (insert, or change existing one)

				int indexLowToRemove = indexHi;
				int indexHiToRemove = indexLow;

				// Find proper value of indexHiToRemove to appropriate index (adjustement that do not require calc of slope)
				if (HullPoints[indexHi].Y.SmallerWithTol(y)) // We can remove 1 or more points without slope calc
				{
					indexHiToRemove = indexHi;
					while (HullPoints[indexHiToRemove + 1].Y.SmallerWithTol(y))
					{
						indexHiToRemove++;
					}
				}

				// Find proper value of indexHiToRemove to appropriate index (adjustement that require calc of slope)
				while (indexHiToRemove + 1 < indexMax)
				{
					double slopeToNewIndexHiToRemovePlus1 = Geometry.CalcSlope(x, y, HullPoints[indexHiToRemove + 1].X,
						HullPoints[indexHiToRemove + 1].Y);

					double slopeToNewIndexHiToRemovePlus2 = Geometry.CalcSlope(x, y, HullPoints[indexHiToRemove + 2].X,
						HullPoints[indexHiToRemove + 2].Y);

					if (slopeToNewIndexHiToRemovePlus2.LargerWithTol( slopeToNewIndexHiToRemovePlus1))
					{
						break;
					}

					indexHiToRemove++;
				}

				// Find proper value of indexLowToRemove to appropriate index (adjustement that require calc of slope)
				while (indexLowToRemove - 1 > 0)
				{
					double slopeToNewIndexLowToRemoveMinus1 = Geometry.CalcSlope(x, y, HullPoints[indexLowToRemove - 1].X,
						HullPoints[indexLowToRemove - 1].Y);

					double slopeToNewIndexLowToRemoveMinus2 = Geometry.CalcSlope(x, y, HullPoints[indexLowToRemove - 2].X,
						HullPoints[indexLowToRemove - 2].Y);

					if (slopeToNewIndexLowToRemoveMinus2.SmallerWithTol(slopeToNewIndexLowToRemoveMinus1))
					{
						break;
					}

					indexLowToRemove--;
				}

				PointInfo<T> newPointInfo;
				if (indexHiToRemove == indexLow) // No point to invalidate after, already calculated slope to next is still valid
				{
                    newPointInfo = new PointInfo<T>(_pointConstructor(x, y), _xGet, _yGet, _pointEqual, slopeToNext);
				}
				else
				{
                    newPointInfo = new PointInfo<T>(_pointConstructor(x, y), _xGet, _yGet, _pointEqual, Geometry.CalcSlope(x, y, HullPoints[indexHiToRemove + 1].X, HullPoints[indexHiToRemove + 1].Y));
				}

				if (indexHiToRemove >= indexLowToRemove) // Any existing hull point become invalide ?
				{
					HullPoints[indexLowToRemove] = newPointInfo;

					if (indexHiToRemove > indexLowToRemove)
					{
						HullPoints.RemoveRange(indexLowToRemove + 1, indexHiToRemove - indexLowToRemove);
					}
				}
				else
				{
					HullPoints.Insert(indexHi, newPointInfo);
				}

				HullPoints[indexLowToRemove - 1].SlopeToNext = Geometry.CalcSlope(x, y, HullPoints[indexLowToRemove - 1].X, HullPoints[indexLowToRemove - 1].Y);
			}
		}

		// ******************************************************************
	}
}
