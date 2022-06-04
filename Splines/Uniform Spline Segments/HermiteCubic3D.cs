// by Freya Holmér (https://github.com/FreyaHolmer/Mathfs)

using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Freya {

	/// <summary>An optimized uniform 3D Cubic hermite segment, with 4 control points</summary>
	[Serializable] public struct HermiteCubic3D : IParamCubicSplineSegment3D {

		const MethodImplOptions INLINE = MethodImplOptions.AggressiveInlining;

		/// <summary>Creates a uniform 3D Cubic hermite segment, from 4 control points</summary>
		/// <param name="p0">The starting point of the curve</param>
		/// <param name="v0">The rate of change (velocity) at the start of the curve</param>
		/// <param name="p1">The end point of the curve</param>
		/// <param name="v1">The rate of change (velocity) at the end of the curve</param>
		public HermiteCubic3D( Vector3 p0, Vector3 v0, Vector3 p1, Vector3 v1 ) {
			( this.p0, this.v0, this.p1, this.v1 ) = ( p0, v0, p1, v1 );
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

		[SerializeField] Vector3 p0, v0, p1, v1;

		/// <summary>The starting point of the curve</summary>
		public Vector3 P0 {
			[MethodImpl( INLINE )] get => p0;
			[MethodImpl( INLINE )] set => _ = ( p0 = value, validCoefficients = false );
		}

		/// <summary>The rate of change (velocity) at the start of the curve</summary>
		public Vector3 V0 {
			[MethodImpl( INLINE )] get => v0;
			[MethodImpl( INLINE )] set => _ = ( v0 = value, validCoefficients = false );
		}

		/// <summary>The end point of the curve</summary>
		public Vector3 P1 {
			[MethodImpl( INLINE )] get => p1;
			[MethodImpl( INLINE )] set => _ = ( p1 = value, validCoefficients = false );
		}

		/// <summary>The rate of change (velocity) at the end of the curve</summary>
		public Vector3 V1 {
			[MethodImpl( INLINE )] get => v1;
			[MethodImpl( INLINE )] set => _ = ( v1 = value, validCoefficients = false );
		}

		#endregion


		#region Coefficients

		[NonSerialized] bool validCoefficients;

		[MethodImpl( INLINE )] void ReadyCoefficients() {
			if( validCoefficients )
				return; // no need to update
			validCoefficients = true;
			curve = CharMatrix.cubicHermite.GetCurve( p0, v0, p1, v1 );
		}

		#endregion

		public BezierCubic3D ToBezier() => new BezierCubic3D( p0, p0 + v0 / 3, p1 - v1 / 3, p1 );

	}

}