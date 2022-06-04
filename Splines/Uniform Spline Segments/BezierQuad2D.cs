// by Freya Holmér (https://github.com/FreyaHolmer/Mathfs)

using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Freya {

	/// <summary>An optimized uniform 2D Quadratic bézier segment, with 3 control points</summary>
	[Serializable] public struct BezierQuad2D : IParamCubicSplineSegment2D {

		const MethodImplOptions INLINE = MethodImplOptions.AggressiveInlining;

		/// <summary>Creates a uniform 2D Quadratic bézier segment, from 3 control points</summary>
		/// <param name="p0">The starting point of the curve</param>
		/// <param name="p1">The middle control point of the curve, sometimes called a tangent point</param>
		/// <param name="p2">The end point of the curve</param>
		public BezierQuad2D( Vector2 p0, Vector2 p1, Vector2 p2 ) {
			( this.p0, this.p1, this.p2 ) = ( p0, p1, p2 );
			validCoefficients = false;
			curve = default;
		}

		Polynomial2D curve;
		public Polynomial2D Curve {
			get {
				ReadyCoefficients();
				return curve;
			}
		}

		#region Control Points

		[SerializeField] Vector2 p0, p1, p2;

		/// <summary>The starting point of the curve</summary>
		public Vector2 P0 {
			[MethodImpl( INLINE )] get => p0;
			[MethodImpl( INLINE )] set => _ = ( p0 = value, validCoefficients = false );
		}

		/// <summary>The middle control point of the curve, sometimes called a tangent point</summary>
		public Vector2 P1 {
			[MethodImpl( INLINE )] get => p1;
			[MethodImpl( INLINE )] set => _ = ( p1 = value, validCoefficients = false );
		}

		/// <summary>The end point of the curve</summary>
		public Vector2 P2 {
			[MethodImpl( INLINE )] get => p2;
			[MethodImpl( INLINE )] set => _ = ( p2 = value, validCoefficients = false );
		}

		/// <summary>Get or set a control point position by index. Valid indices from 0 to 2</summary>
		public Vector2 this[ int i ] {
			get =>
				i switch {
					0 => P0,
					1 => P1,
					2 => P2,
					_ => throw new ArgumentOutOfRangeException( nameof(i), $"Index has to be in the 0 to 2 range, and I think {i} is outside that range you know" )
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
					default: throw new ArgumentOutOfRangeException( nameof(i), $"Index has to be in the 0 to 2 range, and I think {i} is outside that range you know" );
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
			curve = CharMatrix.quadraticBezier.GetCurve( p0, p1, p2 );
		}

		#endregion

		/// <inheritdoc cref="BezierCubic2D.Split(float)"/>
		public BezierQuad2D Split( float t ) {
			Vector2 mid = Vector2.LerpUnclamped( p0, p1, t );
			Vector2 b = Vector2.LerpUnclamped( p1, p2, t );
			Vector2 end = Vector2.LerpUnclamped( mid, b, t );
			return new BezierQuad2D( p0, mid, end );
		}

	}

}