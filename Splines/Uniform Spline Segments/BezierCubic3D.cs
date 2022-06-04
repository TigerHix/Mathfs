// by Freya Holmér (https://github.com/FreyaHolmer/Mathfs)

using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Freya {

	/// <summary>An optimized uniform 3D Cubic bézier segment, with 4 control points</summary>
	[Serializable] public struct BezierCubic3D : IParamCubicSplineSegment3D {

		const MethodImplOptions INLINE = MethodImplOptions.AggressiveInlining;

		/// <summary>Creates a uniform 3D Cubic bézier segment, from 4 control points</summary>
		/// <param name="p0">The starting point of the curve</param>
		/// <param name="p1">The second control point of the curve, sometimes called the start tangent point</param>
		/// <param name="p2">The third control point of the curve, sometimes called the end tangent point</param>
		/// <param name="p3">The end point of the curve</param>
		public BezierCubic3D( Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3 ) {
			( this.p0, this.p1, this.p2, this.p3 ) = ( p0, p1, p2, p3 );
			validCoefficients = false;
			curve = default;
		}

		Polynomial3D curve;
		public Polynomial3D Curve {
			get {
				ReadyCoefficients();
				return curve;
			}
		}

		#region Control Points

		[SerializeField] Vector3 p0, p1, p2, p3;

		/// <summary>The starting point of the curve</summary>
		public Vector3 P0 {
			[MethodImpl( INLINE )] get => p0;
			[MethodImpl( INLINE )] set => _ = ( p0 = value, validCoefficients = false );
		}

		/// <summary>The second control point of the curve, sometimes called the start tangent point</summary>
		public Vector3 P1 {
			[MethodImpl( INLINE )] get => p1;
			[MethodImpl( INLINE )] set => _ = ( p1 = value, validCoefficients = false );
		}

		/// <summary>The third control point of the curve, sometimes called the end tangent point</summary>
		public Vector3 P2 {
			[MethodImpl( INLINE )] get => p2;
			[MethodImpl( INLINE )] set => _ = ( p2 = value, validCoefficients = false );
		}

		/// <summary>The end point of the curve</summary>
		public Vector3 P3 {
			[MethodImpl( INLINE )] get => p3;
			[MethodImpl( INLINE )] set => _ = ( p3 = value, validCoefficients = false );
		}

		/// <summary>Get or set a control point position by index. Valid indices from 0 to 3</summary>
		public Vector3 this[ int i ] {
			get =>
				i switch {
					0 => P0,
					1 => P1,
					2 => P2,
					3 => P3,
					_ => throw new ArgumentOutOfRangeException( nameof(i), $"Index has to be in the 0 to 3 range, and I think {i} is outside that range you know" )
				};
			set {
				switch( i ) {
					case 0:
						P0 = value;
						break;
					case 1:
						P1 = value;
						break;
					case 2:
						P2 = value;
						break;
					case 3:
						P3 = value;
						break;
					default: throw new ArgumentOutOfRangeException( nameof(i), $"Index has to be in the 0 to 3 range, and I think {i} is outside that range you know" );
				}
			}
		}

		#endregion


		#region Coefficients

		[NonSerialized] bool validCoefficients;

		[MethodImpl( INLINE )] void ReadyCoefficients() {
			if( validCoefficients )
				return; // no need to update
			validCoefficients = true;
			curve = CharMatrix.cubicBezier.GetCurve( p0, p1, p2, p3 );
		}

		#endregion


		#region Object Comparison & ToString

		public static bool operator ==( BezierCubic3D a, BezierCubic3D b ) => a.P0 == b.P0 && a.P1 == b.P1 && a.P2 == b.P2 && a.P3 == b.P3;
		public static bool operator !=( BezierCubic3D a, BezierCubic3D b ) => !( a == b );
		public bool Equals( BezierCubic3D other ) => P0.Equals( other.P0 ) && P1.Equals( other.P1 ) && P2.Equals( other.P2 ) && P3.Equals( other.P3 );
		public override bool Equals( object obj ) => obj is BezierCubic3D other && Equals( other );
		public override int GetHashCode() => HashCode.Combine( p0, p1, p2, p3 );

		public override string ToString() => $"({p0}, {p1}, {p2}, {p3})";

		#endregion

		#region Type Casting

		/// <summary>Returns this bezier curve flattened to the Z plane, effectively setting z to 0</summary>
		/// <param name="bezierCubic3D">The 3D curve to cast and flatten on the Z plane</param>
		public static explicit operator BezierCubic2D( BezierCubic3D bezierCubic3D ) {
			return new BezierCubic2D( bezierCubic3D.P0, bezierCubic3D.P1, bezierCubic3D.P2, bezierCubic3D.P3 );
		}

		#endregion

		#region Interpolation

		/// <summary>Returns a linear blend between two bézier curves</summary>
		/// <param name="a">The first spline segment</param>
		/// <param name="b">The second spline segment</param>
		/// <param name="t">A value from 0 to 1 to blend between <c>a</c> and <c>b</c></param>
		public static BezierCubic3D Lerp( BezierCubic3D a, BezierCubic3D b, float t ) =>
			new(
				Vector3.LerpUnclamped( a.p0, b.p0, t ),
				Vector3.LerpUnclamped( a.p1, b.p1, t ),
				Vector3.LerpUnclamped( a.p2, b.p2, t ),
				Vector3.LerpUnclamped( a.p3, b.p3, t )
			);

		/// <summary>Returns a linear blend between two bézier curves, where the tangent directions are spherically interpolated</summary>
		/// <param name="a">The first spline segment</param>
		/// <param name="b">The second spline segment</param>
		/// <param name="t">A value from 0 to 1 to blend between <c>a</c> and <c>b</c></param>
		public static BezierCubic3D Slerp( BezierCubic3D a, BezierCubic3D b, float t ) {
			Vector3 p0 = Vector3.LerpUnclamped( a.p0, b.p0, t );
			Vector3 p3 = Vector3.LerpUnclamped( a.p3, b.p3, t );
			return new BezierCubic3D(
				p0,
				p0 + Vector3.SlerpUnclamped( a.p1 - a.p0, b.p1 - b.p0, t ),
				p3 + Vector3.SlerpUnclamped( a.p2 - a.p3, b.p2 - b.p3, t ),
				p3
			);
		}

		#endregion

		#region Splitting

		/// <inheritdoc cref="BezierCubic2D.Split(float)"/>
		public (BezierCubic3D pre, BezierCubic3D post) Split( float t ) {
			Vector3 a = new Vector3(
				P0.x + ( P1.x - P0.x ) * t,
				P0.y + ( P1.y - P0.y ) * t,
				P0.z + ( P1.z - P0.z ) * t );
			float bx = P1.x + ( P2.x - P1.x ) * t;
			float by = P1.y + ( P2.y - P1.y ) * t;
			float bz = P1.z + ( P2.z - P1.z ) * t;
			Vector3 c = new Vector3(
				P2.x + ( P3.x - P2.x ) * t,
				P2.y + ( P3.y - P2.y ) * t,
				P2.z + ( P3.z - P2.z ) * t );
			Vector3 d = new Vector3(
				a.x + ( bx - a.x ) * t,
				a.y + ( by - a.y ) * t,
				a.z + ( bz - a.z ) * t );
			Vector3 e = new Vector3(
				bx + ( c.x - bx ) * t,
				by + ( c.y - by ) * t,
				bz + ( c.z - bz ) * t );
			Vector3 p = new Vector3(
				d.x + ( e.x - d.x ) * t,
				d.y + ( e.y - d.y ) * t,
				d.z + ( e.z - d.z ) * t );
			return ( new BezierCubic3D( P0, a, d, p ), new BezierCubic3D( p, e, c, P3 ) );
		}

		#endregion

	}

}