﻿using Sandbox;

[Library( "dm_crossbow", Title = "Sniper Rifle" )]
[Hammer.EditorModel( "models/weapons/w_models/w_sniperrifle.vmdl" )]
partial class SniperRifle : BaseDmWeapon
{ 
	public override string ViewModelPath => "models/weapons/v_models/v_sniperrifle_sniper.vmdl";

	public override float PrimaryRate => 1;
	public override int Bucket => 3;
	public override int ClipSize => 1;
	public override float ReloadTime => 1.0f;
	public override AmmoType AmmoType => AmmoType.Crossbow;

	[Net]
	public bool Zoomed { get; set; }

	public override void Spawn()
	{
		base.Spawn();

		AmmoClip = 1;
		SetModel( "models/weapons/w_models/w_sniperrifle.vmdl" );
	}

	public override void AttackPrimary()
	{
		if ( !TakeAmmo( 1 ) )
		{
			DryFire();
			return;
		}

		ShootEffects();

		if ( IsServer )
		using ( Prediction.Off() )
		{
			var bolt = new CrossbowBolt();
			bolt.Position = Owner.EyePos;
			bolt.Rotation = Owner.EyeRot;
			bolt.Owner = Owner;
			bolt.Velocity = Owner.EyeRot.Forward * 100;
		}

		PlaySound( "sniper_shoot" );

		Reload();
	}		    

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		Zoomed = Input.Down( InputButton.Attack2 );
	}

	public override void PostCameraSetup( ref CameraSetup camSetup )
	{
		base.PostCameraSetup( ref camSetup );

		if ( Zoomed )
		{
			camSetup.FieldOfView = 20;
		}
	}

	public override void BuildInput( InputBuilder owner ) 
	{
		if ( Zoomed )
		{
			owner.ViewAngles = Angles.Lerp( owner.OriginalViewAngles, owner.ViewAngles, 0.2f );
		}
	}

	[ClientRpc]
	protected override void ShootEffects()
	{
		Host.AssertClient();

		if ( Owner == Local.Pawn )
		{
			new Sandbox.ScreenShake.Perlin( 0.5f, 4.0f, 1.0f, 0.5f );
		}

		ViewModelEntity?.SetAnimBool( "fire", true );
		CrosshairPanel?.CreateEvent( "fire" );
	}

	public override void Reload()
	{
		PlaySound( "sniper_reload" );

		if ( IsReloading )
			return;

		if ( AmmoClip >= ClipSize )
			return;

		TimeSinceReload = 0;

		if ( Owner is DeathmatchPlayer player )
		{
			if ( player.AmmoCount( AmmoType ) <= 0 )
				return;

			StartReloadEffects();
		}

		IsReloading = true;

		StartReloadEffects();
	}
}
