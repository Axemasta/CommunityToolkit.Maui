﻿namespace CommunityToolkit.Maui.Behaviors;

/// <summary>
/// A behavior that allows you to tint an icon with a specified <see cref="Color"/>.
/// </summary>
public partial class IconTintColorBehavior : PlatformBehavior<View>
{
	/// <summary>
	/// Attached Bindable Property for the <see cref="TintColor"/>.
	/// </summary>
	public static readonly BindableProperty TintColorProperty =
		BindableProperty.Create(nameof(TintColor), typeof(Color), typeof(IconTintColorBehavior), default);
	
	/// <summary>
	/// Attached Bindable Property for the <see cref="TintGradient"/>.
	/// </summary>
	public static readonly BindableProperty TintGradientProperty =
		BindableProperty.Create(nameof(TintGradient), typeof(Brush), typeof(IconTintColorBehavior), default);

	/// <summary>
	/// Property that represents the <see cref="Color"/> that Icon will be tinted.
	/// </summary>
	public Color? TintColor
	{
		get => (Color?)GetValue(TintColorProperty);
		set => SetValue(TintColorProperty, value);
	}

	/// <summary>
	/// Property that represents the <see cref="Brush"/> that Icon will be tinted.
	/// </summary>
	public Brush? TintGradient
	{
		get => (Brush?)GetValue(TintGradientProperty);
		set => SetValue(TintGradientProperty, value);
	}
}