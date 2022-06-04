// by Freya Holmér (https://github.com/FreyaHolmer/Mathfs)

using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Freya {

	/// <summary>An optimized uniform 3D Cubic catmull-rom segment, with 4 control points</summary>
	[Serializable] public struct CatRomCubic3D : IParamCubicSplineSegment3D {

		const MethodImplOptions INLINE = MethodImplOptions.AggressiveInlining;

		/// <summary>Creates a uniform 3D Cubic catmull-rom segment, from 4 control points</summary>
		/// <param name="p0">The first control point of the catmull-rom curve. Note that this point is not included in the curve itself, and only helps to shape it</param>
		/// <param name="p1">The second control point, and the start of the catmull-rom curve</param>
		/// <param name="p2">The third control point, and the end of the catmull-rom curve</param>
		/// <param name="p3">The last control point of the catmull-rom curve. Note that this point is not included in the curve itself, and only helps to shape it</param>
		public CatRomCubic3D( Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3 ) {
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

		/// <summary>The first control point of the catmull-rom curve. Note that this point is not included in the curve itself, and only helps to shape it</summary>
		public Vector3 P0 {
			[MethodImpl( INLINE )] get => p0;
			[MethodImpl( INLINE )] set => _ = ( p0 = value, validCoefficients = false );
		}

		/// <summary>The second control point, and the start of the catmull-rom curve</summary>
		public Vector3 P1 {
			[MethodImpl( INLINE )] get => p1;
			[MethodImpl( INLINE )] set => _ = ( p1 = value, validCoefficients = false );
		}

		/// <summary>The third control point, and the end of the catmull-rom curve</summary>
		public Vector3 P2 {
			[MethodImpl( INLINE )] get => p2;
			[MethodImpl( INLINE )] set => _ = ( p2 = value, validCoefficients = false );
		}

		/// <summary>The last control point of the catmull-rom curve. Note that this point is not included in the curve itself, and only helps to shape it</summary>
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
			curve = CharMatrix.cubicCatmullRom.GetCurve( p0, p1, p2, p3 );
		}

		#endregion


		#region Object Comparison & ToString

		public static bool operator ==( CatRomCubic3D a, CatRomCubic3D b ) => a.P0 == b.P0 && a.P1 == b.P1 && a.P2 == b.P2 && a.P3 == b.P3;
		public static bool operator !=( CatRomCubic3D a, CatRomCubic3D b ) => !( a == b );
		public bool Equals( CatRomCubic3D other ) => P0.Equals( other.P0 ) && P1.Equals( other.P1 ) && P2.Equals( other.P2 ) && P3.Equals( other.P3 );
		public override bool Equals( object obj ) => obj is CatRomCubic3D other && Equals( other );
		public override int GetHashCode() => HashCode.Combine( p0, p1, p2, p3 );

		public override string ToString() => $"({p0}, {p1}, {p2}, {p3})";

		#endregion


		#region Interpolation

		/// <summary>Returns a linear blend between two catmull-rom curves</summary>
		/// <param name="a">The first spline segment</param>
		/// <param name="b">The second spline segment</param>
		/// <param name="t">A value from 0 to 1 to blend between <c>a</c> and <c>b</c></param>
		public static CatRomCubic3D Lerp( CatRomCubic3D a, CatRomCubic3D b, float t ) =>
			new(
				Vector3.LerpUnclamped( a.p0, b.p0, t ),
				Vector3.LerpUnclamped( a.p1, b.p1, t ),
				Vector3.LerpUnclamped( a.p2, b.p2, t ),
				Vector3.LerpUnclamped( a.p3, b.p3, t )
			);

		#endregion

		/// <inheritdoc cref="CatRomCubic2D.ToBezier()"/>
		public BezierCubic3D ToBezier() =>
			new BezierCubic3D(
				p1,
				p1 + ( p2 - p0 ) / 6f,
				p2 + ( p1 - p3 ) / 6f,
				p2
			);

		/// <inheritdoc cref="CatRomCubic2D.ToHermite()"/>
		public HermiteCubic3D ToHermite() =>
			new HermiteCubic3D(
				p1,
				( p2 - p0 ) / 2f,
				p2,
				( p3 - p1 ) / 2f
			);

		/// <inheritdoc cref="CatRomCubic2D.ToBSpline()"/>
		public UBSCubic3D ToBSpline() =>
			new UBSCubic3D(
				( 7 * p0 - 4 * p1 + 5 * p2 - 2 * p3 ) / 6,
				( -2 * p0 + 11 * p1 - 4 * p2 + p3 ) / 6,
				( p0 - 4 * p1 + 11 * p2 - 2 * p3 ) / 6,
				( -2 * p0 + 5 * p1 - 4 * p2 + 7 * p3 ) / 6
			);

	}

}