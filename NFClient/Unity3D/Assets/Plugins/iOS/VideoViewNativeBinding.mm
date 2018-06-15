
#import <FleetVideoUIControls/UIFleetLiveView.h>

extern "C"
{
    void ActivateVideoView(/*const char* deviceId*/)
    {
        UIView* currView = [[[[UIApplication sharedApplication] keyWindow] subviews] lastObject];
        UIFleetLiveView *fleetLiveView;
        fleetLiveView = [[UIFleetLiveView alloc] initWithFrame:CGRectMake(0, 0, currView.frame.size.width, currView.frame.size.height/2)];
        [currView addSubview:fleetLiveView];
        [fleetLiveView setConnectionParm:@"54.199.103.26" port:8899 channel:1 acc:@"admin" pwd:@"admin" deviceId:@"VG500122"];
//        [fleetLiveView setConnectionParm:@"125.227.91.156" port:8899 channel:1 acc:@"admin" pwd:@"admin" deviceId:[NSString stringWithUTF8String:deviceId]];
        [fleetLiveView playVideo];
    }
}
