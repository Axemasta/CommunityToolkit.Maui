using System.ComponentModel;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Platform;
using UIKit;

namespace CommunityToolkit.Maui.Behaviors;

public partial class IconTintColorBehavior
{
	/// <inheritdoc/>
	protected override void OnAttachedTo(View bindable, UIView platformView)
	{
		if (TintColor is not null)
		{
			ApplyTintColor(platformView, bindable, TintColor);
		}
		
		if (TintGradient is not null)
		{
			ApplyTintGradient(platformView, bindable, TintGradient);
		}

		bindable.PropertyChanged += OnElementPropertyChanged;
		this.PropertyChanged += (s, e) =>
		{
			if (e.PropertyName == TintColorProperty.PropertyName)
			{
				ApplyTintColor(platformView, bindable, TintColor);
			}

			if (e.PropertyName == TintGradientProperty.PropertyName)
			{
				ApplyTintGradient(platformView, bindable, TintGradient);
			}
		};
	}

	void OnElementPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if ((e.PropertyName != ImageButton.IsLoadingProperty.PropertyName
			&& e.PropertyName != Image.SourceProperty.PropertyName
			&& e.PropertyName != ImageButton.SourceProperty.PropertyName)
			|| sender is not IImageElement element
			|| (sender as VisualElement)?.Handler?.PlatformView is not UIView platformView)
		{
			return;
		}

		if (!element.IsLoading)
		{
			ApplyTintColor(platformView, (View)element, TintColor);
		}
	}

	/// <inheritdoc/>
	protected override void OnDetachedFrom(View bindable, UIView platformView)
	{
		bindable.PropertyChanged -= OnElementPropertyChanged;
		ClearTintColor(platformView, bindable);
	}

	void ClearTintColor(UIView platformView, View element)
	{
		switch (platformView)
		{
			case UIImageView imageView:
				if (imageView.Image is not null)
				{
					imageView.Image = imageView.Image.ImageWithRenderingMode(UIImageRenderingMode.AlwaysOriginal);
				}

				break;
			case UIButton button:
				if (button.ImageView?.Image is not null)
				{
					var originalImage = button.CurrentImage.ImageWithRenderingMode(UIImageRenderingMode.AlwaysOriginal);
					button.SetImage(originalImage, UIControlState.Normal);
				}

				break;

			default:
				throw new NotSupportedException($"{nameof(IconTintColorBehavior)} only currently supports {nameof(UIButton)} and {nameof(UIImageView)}.");
		}
	}

	void ApplyTintColor(UIView platformView, View element, Color? color)
	{
		if (color is null)
		{
			ClearTintColor(platformView, element);
			return;
		}

		switch (platformView)
		{
			case UIImageView imageView:
				SetUIImageViewTintColor(imageView, element, color);
				break;
			case UIButton button:
				SetUIButtonTintColor(button, element, color);
				break;
			default:
				throw new NotSupportedException($"{nameof(IconTintColorBehavior)} only currently supports {nameof(UIButton)} and {nameof(UIImageView)}.");
		}
	}

	static void SetUIButtonTintColor(UIButton button, View element, Color color)
	{
		if (button.ImageView.Image is null)
		{
			return;
		}

		var templatedImage = button.CurrentImage.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);

		button.SetImage(null, UIControlState.Normal);
		var platformColor = color.ToPlatform();
		button.TintColor = platformColor;
		button.ImageView.TintColor = platformColor;
		button.SetImage(templatedImage, UIControlState.Normal);

	}

	static void SetUIImageViewTintColor(UIImageView imageView, View element, Color color)
	{
		if (imageView.Image is null)
		{
			return;
		}

		imageView.Image = imageView.Image.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
		imageView.TintColor = color.ToPlatform();
	}

	void ApplyTintGradient(UIView platformView, View element, Brush? brush)
	{
		if (brush is null)
		{
			ClearTintColor(platformView, element);
			return;
		}
		
		switch (platformView)
		{
			case UIImageView imageView:
				SetUIImageViewTintColor(imageView, element, brush);
				break;
			case UIButton button:
				// SetUIButtonTintColor(button, element, brush);
				break;
			default:
				throw new NotSupportedException($"{nameof(IconTintColorBehavior)} only currently supports {nameof(UIButton)} and {nameof(UIImageView)}.");
		}
	}
	
	static void SetUIImageViewTintColor(UIImageView imageView, View element, Brush brush)
	{
		if (imageView.Image is null)
		{
			return;
		}

		var renderedImage = imageView.Image.RenderGradient(brush);

		if (renderedImage is not null)
		{
			imageView.Image = renderedImage;
		}
	}
}

static class UIImageExtension
{
	const string backgroundLayer = "BackgroundLayer";
	
	internal static UIImage? RenderGradient(this UIImage image, UIColor[] colors)
	{
		var source = image.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate).CGImage;

		if (source is null)
		{
			return null;
		}

		var size = image.Size;

		var cgColors = new CGColor[colors.Length];

		for (int i = 0; i < colors.Length; i++)
		{
			cgColors[i] = colors[i].CGColor; 
		}
		
		var space = CGColorSpace.CreateDeviceRGB();

		var gradient = new CGGradient(space, cgColors);

		using var renderer = new UIGraphicsImageRenderer(size);
		
		var resultImage = renderer.CreateImage((context =>
		{
			var cgContext = context.CGContext;
			cgContext.TranslateCTM(0, size.Height);
			cgContext.ScaleCTM(1, -1);
			
			cgContext.SetBlendMode(CGBlendMode.Normal);

			var rect = new CGRect(0, 0, size.Width, size.Height);
			
			cgContext.ClipToMask(rect, source);
			cgContext.DrawLinearGradient(gradient, CGPoint.Empty, new CGPoint(0, size.Height), CGGradientDrawingOptions.DrawsAfterEndLocation);
		}));

		return resultImage;
	}
	
	internal static UIImage? RenderGradient(this UIImage image, Brush brush)
	{
		var source = image.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate).CGImage;

		if (source is null)
		{
			return null;
		}

		var size = image.Size;

		var gradient = GetBrushGradient(image, brush);

		using var renderer = new UIGraphicsImageRenderer(size);
		
		var resultImage = renderer.CreateImage((context =>
		{
			var cgContext = context.CGContext;
			cgContext.TranslateCTM(0, size.Height);
			cgContext.ScaleCTM(1, -1);
			
			cgContext.SetBlendMode(CGBlendMode.Normal);

			var rect = new CGRect(0, 0, size.Width, size.Height);
			
			cgContext.ClipToMask(rect, source);
			cgContext.DrawLinearGradient(gradient, CGPoint.Empty, new CGPoint(0, size.Height), CGGradientDrawingOptions.DrawsAfterEndLocation);
		}));

		return resultImage;
	}

	static CGGradient? GetBrushGradient(UIImage? image, Brush brush)
	{
		if (image is null)
		{
			return null;
		}

		if (brush is SolidColorBrush solidColorBrush)
		{
			
		}

		if (brush is LinearGradientBrush linearGradientBrush)
		{
			var p1 = linearGradientBrush.StartPoint;
			var p2 = linearGradientBrush.EndPoint;
			
			

			if (linearGradientBrush.GradientStops is null || linearGradientBrush.GradientStops.Count == 0)
			{
				return null;
			}
			
			var orderedStops = linearGradientBrush.GradientStops.OrderBy(x => x.Offset).ToList();
			
			var space = CGColorSpace.CreateDeviceRGB();
			var colors = GetCAGradientLayerColors(orderedStops);
			// var locations = GetCAGradientLayerLocations(orderedStops);

			var gradient = new CGGradient(space, colors);

			// if (linearGradientBrush.GradientStops != null && linearGradientBrush.GradientStops.Count > 0)
			// {
			// 	var orderedStops = linearGradientBrush.GradientStops.OrderBy(x => x.Offset).ToList();
			// 	linearGradientLayer.Colors = GetCAGradientLayerColors(orderedStops);
			// 	linearGradientLayer.Locations = GetCAGradientLayerLocations(orderedStops);
			// }

			return gradient;
		}

		if (brush is RadialGradientBrush radialGradientBrush)
		{
			
		}

		return null;
	}
	
	static CGColor[] GetCAGradientLayerColors(List<GradientStop> gradientStops)
	{
		if (gradientStops == null || gradientStops.Count == 0)
		{
			return new CGColor[0];
		}

		CGColor[] colors = new CGColor[gradientStops.Count];

		int index = 0;
		foreach (var gradientStop in gradientStops)
		{
			if (gradientStop.Color == Colors.Transparent)
			{
				var color = gradientStops[index == 0 ? index + 1 : index - 1].Color;
				CGColor nativeColor = color.ToPlatform().ColorWithAlpha(0.0f).CGColor;
				colors[index] = nativeColor;
			}
			else
			{
				colors[index] = gradientStop.Color.ToCGColor();
			}

			index++;
		}

		return colors;
	}
	
	static NSNumber[] GetCAGradientLayerLocations(List<GradientStop> gradientStops)
	{
		if (gradientStops == null || gradientStops.Count == 0)
		{
			return new NSNumber[0];
		}

		if (gradientStops.Count > 1 && gradientStops.Any(gt => gt.Offset != 0))
		{
			return gradientStops.Select(x => new NSNumber(x.Offset)).ToArray();
		}
		else
		{
			int itemCount = gradientStops.Count;
			int index = 0;
			float step = 1.0f / itemCount;

			NSNumber[] locations = new NSNumber[itemCount];

			foreach (var gradientStop in gradientStops)
			{
				float location = step * index;
				bool setLocation = !gradientStops.Any(gt => gt.Offset > location);

				if (gradientStop.Offset == 0 && setLocation)
				{
					locations[index] = new NSNumber(location);
				}
				else
				{
					locations[index] = new NSNumber(gradientStop.Offset);
				}

				index++;
			}

			return locations;
		}
	}
}