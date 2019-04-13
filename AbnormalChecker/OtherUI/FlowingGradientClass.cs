
using Android.Graphics.Drawables;
using Android.Widget;

public class FlowingGradientClass {

    int Duration=4000;
    RelativeLayout rl;
    LinearLayout ll;
    ImageView im;
    int alphaint;
    int d;
    AnimationDrawable frameAnimation;

    public FlowingGradientClass setTransitionDuration(int time) {
        this.Duration = time;

        return this;
    }
    public FlowingGradientClass onLinearLayout(LinearLayout ll) {
        this.ll = ll;
        return this;
    }

    public FlowingGradientClass onImageView(ImageView im) {
        this.im = im;
        return this;
    }

    public FlowingGradientClass onRelativeLayout(RelativeLayout rl) {
        this.rl = rl;
        return this;
    }
    public FlowingGradientClass start() {

        if(ll!=null){
            ll.SetBackgroundResource(d);
        }
        else if(rl!=null){
            rl.SetBackgroundResource(d);
        }
        else if(im!=null){
            im.SetBackgroundResource(d);
        }
        if(ll!=null){
            frameAnimation = (AnimationDrawable) ll.Background;
        }
        else if(rl!=null){
            frameAnimation = (AnimationDrawable) rl.Background;
        }
        else if(im!=null){
            frameAnimation = (AnimationDrawable) im.Background;
        }
        frameAnimation.SetEnterFadeDuration(Duration);
        frameAnimation.SetExitFadeDuration(Duration);
        frameAnimation.Start();

        return this;
    }

    public FlowingGradientClass setBackgroundResource(int d) {
        this.d = d;
        return this;
    }
    public FlowingGradientClass setAlpha(int alphaint) {
        this.alphaint = alphaint;
        frameAnimation.SetAlpha(alphaint);
        return this;
    }

}