//
// AircraftClassification.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013 Jeffrey Stedfast
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//

namespace FlightLog {
	public enum AircraftClassification {
		#region Airplane
		[HumanReadableName ("Single-Engine Land")]
		SingleEngineLand         = AircraftCategory.Airplane,
		[HumanReadableName ("Multi-Engine Land")]
		MultiEngineLand,
		[HumanReadableName ("Single-Engine Sea")]
		SingleEngineSea,
		[HumanReadableName ("Multi-Engine Sea")]
		MultiEngineSea,
		#endregion

		#region Rotorcraft
		Helicoptor               = AircraftCategory.Rotorcraft,
		Gryoplane,
		#endregion

		#region Glider
		Glider                   = AircraftCategory.Glider,
		#endregion

		#region LighterThanAir
		Airship                  = AircraftCategory.LighterThanAir,
		Balloon,
		#endregion

		#region PoweredLift
		[HumanReadableName ("Powered-Lift")]
		PoweredLift              = AircraftCategory.PoweredLift,
		#endregion

		#region PoweredParachute
		[HumanReadableName ("Powered-Parachute Land")]
		PoweredParachuteLand     = AircraftCategory.PoweredParachute,
		[HumanReadableName ("Powered-Parachute Sea")]
		PoweredParachuteSea,
		#endregion

		#region WeightShiftControl
		[HumanReadableName ("Weight-Shift-Control Land")]
		WeightShiftControlLand   = AircraftCategory.WeightShiftControl,
		[HumanReadableName ("Weight-Shift-Control Sea")]
		WeightShiftControlSea,
		#endregion
	}
}
