
using System;
using System.Drawing;

namespace ImageBoxExtensions
{
    /// <summary>
    /// This is a wrapper for the original System.Drawing animator. See <see cref="ImageAnimator"/>.
    /// </summary>
    public class DefaultGifAnimator: GifAnimator {

        /// <summary>
        /// Updates the time frame for this image.
        /// </summary>
        /// <param name="image"></param>
        public void UpdateFrames(Image image) {
            ImageAnimator.UpdateFrames(image);
        }

        /// <summary>
        /// Stops updating frames for the given image. 
        /// </summary>
        /// <param name="image"></param>
        /// <param name="eventHandler"></param>
        public void StopAnimate(Image image, EventHandler eventHandler) {
            ImageAnimator.StopAnimate(image, eventHandler);
        }

        /// <summary>
        /// Animates the given image. 
        /// </summary>
        /// <param name="image"></param>
        /// <param name="eventHandler"></param>
        public void Animate(Image image, EventHandler eventHandler) {
            ImageAnimator.Animate(image, eventHandler);
        }

        /// <summary>
        /// Determines whether an image can be animated.
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public bool CanAnimate(Image image) {
            return ImageAnimator.CanAnimate(image);
        }
    }
}