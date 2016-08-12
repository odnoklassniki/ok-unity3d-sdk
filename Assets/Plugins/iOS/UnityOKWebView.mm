#import <UIKit/UIKit.h>

#ifndef NSFoundationVersionNumber_iOS_7_1
#define NSFoundationVersionNumber_iOS_7_1 1047.25
#endif

#define BELOW_IOS_8 (NSFoundationVersionNumber <= NSFoundationVersionNumber_iOS_7_1)

extern UIViewController *UnityGetGLViewController();
extern ScreenOrientation UnityCurrentOrientation();

extern "C" void UnitySendMessage(const char *, const char *, const char *);

@interface WebViewToolBar : UIToolbar
@property (nonatomic, retain) UIBarButtonItem *btnNext;
@property (nonatomic, retain) UIBarButtonItem *btnBack;
@property (nonatomic, retain) UIBarButtonItem *btnReload;
@property (nonatomic, retain) UIBarButtonItem *btnDone;
@end

@implementation WebViewToolBar
-(void)dealloc {
}
@end

@interface WebSpinner : UIView
@property (nonatomic, retain) UIActivityIndicatorView *indicator;
@property (nonatomic, retain) UILabel *textLabel;
-(id) initWithFrame:(CGRect)frame;
-(void) show;
-(void) hide;
@end

@implementation WebSpinner
-(id) initWithFrame:(CGRect)frame {
	self = [super initWithFrame:frame];
	if (self) {
		self.backgroundColor = [UIColor colorWithRed:0 green:0 blue:0 alpha:0.5];
		self.clipsToBounds = YES;
		self.layer.cornerRadius = 10.0;

		_indicator = [[UIActivityIndicatorView alloc] initWithActivityIndicatorStyle:UIActivityIndicatorViewStyleWhiteLarge];

		_indicator.frame = (CGRect){ frame.size.width / 2 - _indicator.frame.size.width / 2,
									frame.size.height / 2 - _indicator.frame.size.height / 2 - 10,
									_indicator.bounds.size.width,
									_indicator.bounds.size.height};
		[self addSubview:_indicator];

		_textLabel = [[UILabel alloc] initWithFrame:CGRectMake(0, frame.size.height - 22 * 2, frame.size.width, 22)];
		_textLabel.backgroundColor = [UIColor clearColor];
		_textLabel.textColor = [UIColor whiteColor];
		_textLabel.adjustsFontSizeToFitWidth = YES;
		_textLabel.textAlignment = NSTextAlignmentCenter;
		_textLabel.text = @"Loading...";
		[self addSubview:_textLabel];

		UITapGestureRecognizer *tap = [[UITapGestureRecognizer alloc] initWithTarget:self action:@selector(hide)];
		[self addGestureRecognizer:tap];
	}
	return self;
}

-(void) show {
	self.hidden = NO;
	[self.indicator startAnimating];
}

-(void) hide {
	[self.indicator stopAnimating];
	self.hidden = YES;
}

-(void)dealloc {
}
@end

@class WebView;
@interface WebViewManager : NSObject
+ (WebViewManager *) sharedManager;
- (void)webViewDone:(WebView *)webView;
@end

@interface WebView : UIWebView
@property (nonatomic, retain) WebViewToolBar *toolBar;
@property (nonatomic, retain) WebSpinner *spinner;
@property (nonatomic, assign) UIEdgeInsets insets;

@property (nonatomic, assign) BOOL showSpinnerWhenLoading;
@property (nonatomic, copy) NSString *currentUrl;

@property (nonatomic, retain) NSMutableArray *schemes;

-(id) initWithFrame:(CGRect)frame;
-(void) btnDonePressed:(id)sender;
-(void) updateToolBtn;
-(void) changeToInsets:(UIEdgeInsets)insets targetOrientation:(ScreenOrientation)orientation;
-(void) setBounces:(BOOL)bounces;

@end

@interface NSUserDefaults(UnRegisterDefaults)
- (void)uwv_unregisterDefaultForKey:(NSString *)defaultName;
@end

@implementation NSUserDefaults (UnRegisterDefaults)

- (void)uwv_unregisterDefaultForKey:(NSString *)defaultName {
	NSDictionary *registeredDefaults = [[NSUserDefaults standardUserDefaults] volatileDomainForName:NSRegistrationDomain];
	if ([registeredDefaults objectForKey:defaultName] != nil) {
		NSMutableDictionary *mutableCopy = [NSMutableDictionary dictionaryWithDictionary:registeredDefaults];
		[mutableCopy removeObjectForKey:defaultName];
		[self uwv_replaceRegisteredDefaults:mutableCopy];
	}
}

- (void)uwv_replaceRegisteredDefaults:(NSDictionary *)dictionary {
	[[NSUserDefaults standardUserDefaults] setVolatileDomain:dictionary forName:NSRegistrationDomain];
}

@end

@implementation WebView
-(id) initWithFrame:(CGRect)frame {
	self = [super initWithFrame:frame];
	if (self) {
		CGRect toolBarFrame = CGRectMake(0, frame.size.height - 44, frame.size.width, 44);
		_toolBar = ({
			WebViewToolBar *toolBar = [[WebViewToolBar alloc] initWithFrame:toolBarFrame];

			UIBarButtonItem *back = [[UIBarButtonItem alloc] initWithBarButtonSystemItem:UIBarButtonSystemItemRewind target:self action:@selector(goBack)];
			UIBarButtonItem *forward = [[UIBarButtonItem alloc] initWithBarButtonSystemItem:UIBarButtonSystemItemFastForward target:self action:@selector(goForward)];
			UIBarButtonItem *reload = [[UIBarButtonItem alloc] initWithBarButtonSystemItem:UIBarButtonSystemItemRefresh target:self action:@selector(btnReloadPressed:)];
			UIBarButtonItem *space = [[UIBarButtonItem alloc] initWithBarButtonSystemItem:UIBarButtonSystemItemFlexibleSpace target:self action:nil];
			UIBarButtonItem *done = [[UIBarButtonItem alloc] initWithBarButtonSystemItem:UIBarButtonSystemItemDone target:self action:@selector(btnDonePressed:)];

			toolBar.items = @[back,forward,reload,space,done];

			toolBar.btnBack = back;
			toolBar.btnNext = forward;
			toolBar.btnReload = reload;
			toolBar.btnDone = done;

			toolBar.hidden = YES;

			toolBar;
		});

		_schemes = [[NSMutableArray alloc] initWithObjects:@"Webview", nil];

		_showSpinnerWhenLoading = YES;

		_spinner = ({
			WebSpinner *spinner = [[WebSpinner alloc] initWithFrame:CGRectMake(frame.size.width / 2 - 65, frame.size.height / 2 - 65, 130, 130)];
			[spinner hide];
			spinner;
		});

		[self setBounces:NO];
		[self updateToolBtn];

	}
	return self;
}

-(void) addToView:(UIView *)unityView {
	[unityView addSubview:self];
	[unityView addSubview:self.toolBar];
	[unityView addSubview:self.spinner];
}

-(void) removeFromView {
	[_toolBar removeFromSuperview];
	[_spinner removeFromSuperview];
}

-(void)setBounces:(BOOL)bounces {
	UIScrollView* sv = nil;
	for(UIView* view in self.subviews){
		if([view isKindOfClass:[UIScrollView class]]){
			sv = (UIScrollView*)view;
			sv.bounces = bounces;
		}
	}
}

-(void) btnDonePressed:(id)sender {
	[[WebViewManager sharedManager] webViewDone:self];
}

-(void) btnReloadPressed:(id)sender {
	if (!self.loading) {
		[self reload];
	} else {
		NSLog(@"WebView can not reload because some content is being loading right now.");
	}
}

-(void) updateToolBtn {
	self.toolBar.btnBack.enabled = [self canGoBack];
	self.toolBar.btnNext.enabled = [self canGoForward];
}

-(void)changeToInsets:(UIEdgeInsets)insets targetOrientation:(ScreenOrientation)orientation {
	UIView *unityView = UnityGetGLViewController().view;
	CGRect viewRect = unityView.frame;

	if (orientation == landscapeLeft || orientation == landscapeRight) {
		if (BELOW_IOS_8) {
			viewRect = CGRectMake(viewRect.origin.x, viewRect.origin.y, viewRect.size.height, viewRect.size.width);
			self.toolBar.frame = CGRectMake(0, unityView.frame.size.width - 44, unityView.frame.size.height, 44);
			self.spinner.frame = CGRectMake(unityView.frame.size.height / 2 - 65, unityView.frame.size.width / 2 - 65, 130, 130);
		} else {
			self.toolBar.frame = CGRectMake(0, unityView.frame.size.height - 44, unityView.frame.size.width, 44);
			self.spinner.frame = CGRectMake(unityView.frame.size.width / 2 - 65, unityView.frame.size.height / 2 - 65, 130, 130);
		}
	} else {
		self.toolBar.frame = CGRectMake(0, unityView.frame.size.height - 44, unityView.frame.size.width, 44);
		self.spinner.frame = CGRectMake(unityView.frame.size.width / 2 - 65, unityView.frame.size.height / 2 - 65, 130, 130);
	}

	CGRect f = CGRectMake(insets.left,
						  insets.top,
						  viewRect.size.width - insets.left - insets.right,
						  viewRect.size.height - insets.top - insets.bottom);
	self.frame = f;
	self.insets = insets;
}

-(void)dealloc {
	[self removeFromView];
}
@end

@interface WebViewManager()<UIWebViewDelegate> {
	NSMutableDictionary *_webViewDic;
	ScreenOrientation _orientationBeforeFullScreen;
	BOOL _multipleOrientation;
}
@end

@implementation WebViewManager
+ (WebViewManager *) sharedManager {
	static dispatch_once_t once;
	static WebViewManager *instance;
	dispatch_once(&once, ^ { instance = [[WebViewManager alloc] init]; });
	return instance;
}

-(instancetype) init {
	self = [super init];
	if (self) {
		_webViewDic = [[NSMutableDictionary alloc] init];
		[self checkOrientationSupport];
	}
	return self;
}

-(void) checkOrientationSupport {
	NSArray *arr = [[[NSBundle mainBundle] infoDictionary] objectForKey:@"UISupportedInterfaceOrientations"];
	__block BOOL portraitOrientation = NO;
	__block BOOL landspaceOrientation = NO;
	
	[arr enumerateObjectsUsingBlock:^(NSString *orientation, NSUInteger idx, BOOL *stop) {
		if ([orientation rangeOfString:@"Portrait"].location != NSNotFound) {
			portraitOrientation = YES;
		} else if ([orientation rangeOfString:@"Landscape"].location != NSNotFound) {
			landspaceOrientation = YES;
		}
		
		if (portraitOrientation && landspaceOrientation) {
			_multipleOrientation = YES;
			*stop = YES;
		}
	}];
}

-(void) addManagedWebView:(WebView *)webView forName:(NSString *)name {
	if (![_webViewDic objectForKey:name]) {
		[_webViewDic setObject:webView forKey:name];
	} else {
		NSLog(@"Duplicated name. Something goes wrong: %@", name);
	}
}

-(void) addManagedWebViewName:(NSString *)name insets:(UIEdgeInsets)insets {
	UIView *unityView = UnityGetGLViewController().view;
	WebView *webView = [[WebView alloc] initWithFrame:unityView.frame];
	webView.mediaPlaybackRequiresUserAction = NO;

	[self changeWebView:webView insets:insets];
	webView.delegate = self;
	webView.hidden = YES;

	[self addManagedWebView:webView forName:name];

	[webView addToView:unityView];
}

-(void) changeWebViewName:(NSString *)name insets:(UIEdgeInsets)insets {
	WebView *webView = [_webViewDic objectForKey:name];
	[self changeWebView:webView insets:insets];
}

-(void) changeWebView:(WebView *)webView insets:(UIEdgeInsets)insets {
	[webView changeToInsets:insets targetOrientation:UnityCurrentOrientation()];
}

-(void) webviewName:(NSString *)name beginLoadURL:(NSString *)urlString {
	WebView *webView = [_webViewDic objectForKey:name];
	NSURL *url = [NSURL URLWithString:urlString];
	NSURLRequest *request = [NSURLRequest requestWithURL:url];

	[webView loadRequest:request];
}

-(void) webViewNameReload:(NSString *)name {
	WebView *webView = [_webViewDic objectForKey:name];
	[webView reload];
}

-(void) webViewNameStop:(NSString *)name {
	WebView *webView = [_webViewDic objectForKey:name];
	if ([webView isLoading]) {
		[webView stopLoading];
	}
}

-(void) webViewNameCleanCache:(NSString *)name {
	WebView *webView = [_webViewDic objectForKey:name];
	[[NSURLCache sharedURLCache] removeCachedResponseForRequest:webView.request];
}

-(void) webViewNameCleanCookies:(NSString *)name {
	
	NSHTTPCookie *cookie;
	NSHTTPCookieStorage *cookieJar = [NSHTTPCookieStorage sharedHTTPCookieStorage];
	
	for (cookie in [cookieJar cookies]) {
		[cookieJar deleteCookie:cookie];
	}
	
	[[NSUserDefaults standardUserDefaults] synchronize];
}

-(void) webViewName:(NSString *)name show:(BOOL)show {
	WebView *webView = [_webViewDic objectForKey:name];
	webView.hidden = !show;
	
	if (!show) {
		[webView.spinner hide];
	}
}

-(void) removeWebViewName:(NSString *)name {
	WebView *webView = [_webViewDic objectForKey:name];
	webView.delegate = nil;
	
	[webView removeFromSuperview];
	
	[_webViewDic removeObjectForKey:name];

}

-(void) updateBackgroundWebViewName:(NSString *)name transparent:(BOOL)transparent {
	WebView *webView = [_webViewDic objectForKey:name];
	webView.opaque = !transparent;
	webView.backgroundColor = transparent ? [UIColor clearColor] : [UIColor whiteColor];
	for (UIView* subView in [webView subviews]) {
		if ([subView isKindOfClass:[UIScrollView class]]) {
			for (UIView* shadowView in [subView subviews]) {
				if ([shadowView isKindOfClass:[UIImageView class]]) {
					[shadowView setHidden:transparent];
				}
			}
		}
	}
}

-(void) webViewName:(NSString *)name showToolBarAnimate:(BOOL)animate {
	WebView *webView = [_webViewDic objectForKey:name];
	if (webView.toolBar.hidden) {
		if (animate) {
			CGRect oldFrame = webView.toolBar.frame;
			webView.toolBar.frame = CGRectOffset(oldFrame, 0, oldFrame.size.height);
			webView.toolBar.hidden = NO;
			[UIView animateWithDuration:0.4 animations:^{
				webView.toolBar.frame = oldFrame;
			}];
		} else {
			webView.toolBar.hidden = NO;
		}
	}
}

-(void) webViewName:(NSString *)name hideToolBarAnimate:(BOOL)animate {
	WebView *webView = [_webViewDic objectForKey:name];
	if (!webView.toolBar.hidden) {
		if (animate) {
			CGRect oldFrame = webView.toolBar.frame;
			[UIView animateWithDuration:0.4 animations:^{
				webView.toolBar.frame = CGRectOffset(oldFrame, 0, oldFrame.size.height);
			} completion:^(BOOL finished) {
				webView.toolBar.hidden = YES;
				webView.toolBar.frame = oldFrame;
			}];
		} else {
			webView.toolBar.hidden = YES;
		}
	}
}

-(void) goBackWebViewName:(NSString *)name {
	WebView *webView = [_webViewDic objectForKey:name];
	[webView goBack];
}

-(void) goForwardWebViewName:(NSString *)name {
	WebView *webView = [_webViewDic objectForKey:name];
	[webView goForward];
}

-(void) webViewName:(NSString *)name setZoomEnable:(BOOL)enable {
	WebView *webView = [_webViewDic objectForKey:name];
	webView.scalesPageToFit = enable;
}

-(void) webViewName:(NSString *)name setBounces:(BOOL)bounces {
	WebView *webView = [_webViewDic objectForKey:name];
	[webView setBounces:bounces];
}

-(void) webViewName:(NSString *)name loadHTMLString:(NSString *)htmlString baseURLString:(NSString *)baseURL {
	WebView *webView = [_webViewDic objectForKey:name];
	[webView loadHTMLString:htmlString baseURL:[NSURL URLWithString:baseURL]];
}

-(void) webViewName:(NSString *)name setSpinnerShowWhenLoading:(BOOL)show {
	WebView *webView = [_webViewDic objectForKey:name];
	webView.showSpinnerWhenLoading = show;
}

-(void) webViewName:(NSString *)name setSpinnerText:(NSString *)text {
	WebView *webView = [_webViewDic objectForKey:name];
	if (text) {
		webView.spinner.textLabel.text = text;
	}
}

-(NSString *) webViewName:(WebView *)webView {
	NSString *webViewName = [[_webViewDic allKeysForObject:webView] lastObject];
	if (!webViewName) {
		NSLog(@"Did not find the webview: %@",webViewName);
	}
	return webViewName;
}

- (void)webViewName:(NSString *)name addUrlScheme:(NSString *)scheme {
	WebView *webView = [_webViewDic objectForKey:name];
	if (![webView.schemes containsObject:scheme]) {
		[webView.schemes addObject:scheme];
	}
}

- (void)webViewName:(NSString *)name removeUrlScheme:(NSString *)scheme {
	WebView *webView = [_webViewDic objectForKey:name];
	if ([webView.schemes containsObject:scheme]) {
		[webView.schemes removeObject:scheme];
	}
}

- (void)webViewDidStartLoad:(WebView *)webView {
	if (webView.showSpinnerWhenLoading && !webView.hidden) {
		[webView.spinner show];
	}
}

- (void)webViewDidFinishLoad:(WebView *)webView {
	[webView.spinner hide];
	NSString *webViewName = [self webViewName:webView];
	[webView updateToolBtn];

	webView.currentUrl = webView.request.mainDocumentURL.absoluteString;
	UnitySendMessage([webViewName UTF8String], "LoadComplete", "");
}

- (void)webView:(WebView *)webView didFailLoadWithError:(NSError *)error {
	[webView.spinner hide];
	NSString *webViewName = [self webViewName:webView];
	[webView updateToolBtn];

	webView.currentUrl = webView.request.mainDocumentURL.absoluteString;
	UnitySendMessage([webViewName UTF8String], "LoadComplete", [error.localizedDescription UTF8String]);
}

- (void)webViewDone:(WebView *)webView {
	[webView.spinner hide];
	NSString *webViewName = [self webViewName:webView];
}

-(NSString *) webViewNameGetCurrentUrl:(NSString *)name {
	WebView *webView = [_webViewDic objectForKey:name];
	return webView.currentUrl ?: @"";
}

-(BOOL)webView:(WebView *)webView shouldStartLoadWithRequest:(NSURLRequest *)request navigationType:(UIWebViewNavigationType)navigationType {
	NSString *webViewName = [self webViewName:webView];

	__block BOOL canResponse = NO;
	[webView.schemes enumerateObjectsUsingBlock:^(NSString *scheme, NSUInteger idx, BOOL *stop) {
		if ([[request.URL absoluteString] rangeOfString:[scheme stringByAppendingString:@"://"]].location == 0) {
			canResponse = YES;
			*stop = YES;
		}
	}];

	if (canResponse) {
		NSString *rawMessage = [NSString stringWithFormat:@"%@",request.URL];
		return NO;
	}
	return YES;
}

-(void) orientationChanged:(NSNotification *)noti {
	[_webViewDic enumerateKeysAndObjectsUsingBlock:^(id key, id obj, BOOL *stop) {
		WebView *webView = (WebView *)obj;
		[webView changeToInsets:webView.insets targetOrientation:UnityCurrentOrientation()];
	}];
}

@end

NSString* WebViewMakeNSString (const char* string) {
	if (string) {
		return [NSString stringWithUTF8String: string];
	} else {
		return [NSString stringWithUTF8String: ""];
	}
}

char* WebViewMakeCString(NSString *str) {
	const char* string = [str UTF8String];
	if (string == NULL) {
		return NULL;
	}

	char* res = (char*)malloc(strlen(string) + 1);
	strcpy(res, string);
	return res;
}

extern "C" {
	void _Init(const char *name);
	void _Resize(const char *name);
	void _Load(const char *name, const char *url);

	void _Show(const char *name);
	void _Hide(const char *name);
	
	void _ClearCookies(const char *name);

	void _Destroy(const char *name);
}

void _Init(const char *name) {
	UIEdgeInsets insets = UIEdgeInsetsMake(0, 0, 0, 0);
	[[WebViewManager sharedManager] addManagedWebViewName:WebViewMakeNSString(name) insets:insets];
}

void _Resize(const char *name) {
	UIEdgeInsets insets = UIEdgeInsetsMake(0, 0, 0, 0);
	[[WebViewManager sharedManager] changeWebViewName:WebViewMakeNSString(name) insets:insets];
}

void _Load(const char *name, const char *url) {
	[[WebViewManager sharedManager] webviewName:WebViewMakeNSString(name)
									  beginLoadURL:WebViewMakeNSString(url)];
}

void _Show(const char *name) {
	[[WebViewManager sharedManager] webViewName:WebViewMakeNSString(name) show:YES];
}

void _Hide(const char *name) {
	[[WebViewManager sharedManager] webViewName:WebViewMakeNSString(name) show:NO];
}

void _ClearCookies(const char *name) {
	[[WebViewManager sharedManager] webViewNameCleanCookies:WebViewMakeNSString(name)];
}

void _Destroy(const char *name) {
	[[WebViewManager sharedManager] removeWebViewName:WebViewMakeNSString(name)];
}