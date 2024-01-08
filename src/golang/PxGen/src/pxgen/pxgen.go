package pxgen

import (
	"bytes"
	"encoding/json"
	"errors"
	"fmt"
	"io/ioutil"
	"math/rand"
	"net/url"
	"strconv"
	"strings"
	"time"

	"github.com/fatih/color"
	"github.com/google/go-querystring/query"
	"github.com/google/uuid"
	pxutils "github.com/incizzle/perimeterx-utils-go"
	"github.com/robertkrimen/otto"
	http "github.com/zMrKrabz/fhttp"
)

type SiteData struct {
	SiteHostName string
	Tag          string
	Ft           string
	AppID        string
	URL          string
}
type PXStruct struct {
	T string      `json:"t"`
	D interface{} `json:"d"`
}
type SensorDataStruct struct {
	Payload string `json:"payload,omitempty" url:"payload,omitempty"`
	AppID   string `json:"appId,omitempty" url:"appId,omitempty"`
	Tag     string `json:"tag,omitempty" url:"tag,omitempty"`
	Uuid    string `json:"uuid,omitempty" url:"uuid,omitempty"`
	Ft      string `json:"ft,omitempty" url:"ft,omitempty"`
	Seq     int    `json:"seq,omitempty" url:"seq,omitempty"`
	En      string `json:"en,omitempty" url:"en,omitempty"`
	Cs      string `json:"cs,omitempty" url:"cs,omitempty"`
	Pc      string `json:"pc,omitempty" url:"pc,omitempty"`
	Sid     string `json:"sid,omitempty" url:"sid,omitempty"`
	Vid     string `json:"vid,omitempty" url:"vid,omitempty"`
	Rsc     int    `json:"rsc,omitempty" url:"rsc,omitempty"`
}

type PXDataStruct struct {
	SID        string `json:"sid" form:"sid"`
	VID        string `json:"vid" form:"vid"`
	CS         string `json:"cs" form:"cs"`
	PX3        string `json:"px3"`
	PXDE       string `json:"pxde"`
	STS        string `json:"sts" form:"sts"`
	CLS1       string `json:"cls1" form:"cls1"`
	CLS2       string `json:"cls2" form:"cls2"`
	UUID       string `json:"uuid" form:"uuid"`
	Grecaptcha string `json:"grecaptcha" form:"grecaptcha"`
	DRC        string `json:"drc" form:"drc"`
	WCS        string `json:"wcs" form:"wcs"`
	UserAgent  string `json:"userAgent" form:"userAgent"`
}

var UserAgentList []string

//var siteData = map[string]SiteData{}
var tasklog *color.Color
var errorlog *color.Color
var successlog *color.Color

//var CapMonsterAPIKey string

func Initialize() {
	UserAgentList = []string{"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.77 Safari/537.36", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.77 Safari/537.36", "Mozilla/5.0 (Windows NT 10.0) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.77 Safari/537.36", "Mozilla/5.0 (Macintosh; Intel Mac OS X 11_4) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.77 Safari/537.36", "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.77 Safari/537.36", "Mozilla/5.0 (Linux; Android 10) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.77 Mobile Safari/537.36"}
	tasklog = color.New(color.FgCyan).Add(color.Bold)
	errorlog = color.New(color.FgRed).Add(color.Bold)
	successlog = color.New(color.FgGreen).Add(color.Bold)
	//CapMonsterAPIKey = "f350b528306832a2d9e397376894754b"
	////siteData = make(map[string]SiteData)
	//siteData["walmart"] = SiteData{
	//	Tag:   "v6.5.5",
	//	Ft:    "202",
	//	AppID: "PXu6b0qd2S",
	//	URL:   "https://www.walmart.com/terra-firma/item/910826028",
	//	//URL:   "https://www.walmart.com/",
	//}
	//siteData["bh"] = SiteData{
	//	Tag:   "v6.5.0",
	//	Ft:    "196",
	//	AppID: "PX3D8mkYG1",
	//	URL:   "https://www.bhphotovideo.com/",
	//}
	//client = &http.Client{}
	rand.Seed(time.Now().Unix())

}

func SetProxy(proxy string) (*http.Client, error) {

	if proxy == "" {
		client := &http.Client{
			Transport: &http.Transport{
				ForceAttemptHTTP2: true,
			},
		}
		return client, nil
	}
	proxySplit := strings.Split(proxy, ":")
	proxyAddress := ""
	if len(proxySplit) > 2 {
		proxyAddress = "http://" + proxySplit[2] + ":" + proxySplit[3] + "@" + proxySplit[0] + ":" + proxySplit[1]
	} else if len(proxySplit) == 2 {
		proxyAddress = "http://" + proxySplit[0] + ":" + proxySplit[1]
	} else {
		logerr(fmt.Sprintf("Invalid proxy %s", proxy))
		return nil, errors.New("invalid proxy")
	}
	proxyURL, err := url.Parse(proxyAddress)
	if err != nil {
		logerr(fmt.Sprintf("Invalid proxy %s", proxyURL))
		return nil, errors.New("invalid proxy")
	}
	logf(fmt.Sprintf("Using Proxy URL: %s", proxyAddress))
	client := &http.Client{
		Transport: &http.Transport{
			ForceAttemptHTTP2: true,
			Proxy:             http.ProxyURL(proxyURL),
		},
	}
	client.Timeout = time.Second * 15
	return client, nil
}

func TestSite(siteInfo *SiteData) bool {
	client, _ := SetProxy("")
	logf(fmt.Sprintf("Testing Gen for %s", strings.Title(siteInfo.SiteHostName)))
	emptyData := PXDataStruct{}
	//emptyData.SID = "c219be40-b398-11eb-aadb-4dfefb9156d8"
	//emptyData.VID = "c21995df-b398-11eb-9a7b-0242ac120006"
	//emptyData.UUID = "c221ddb4-b398-11eb-b6ca-04d9f5f4def4"
	sensor := GetPX2(&emptyData, siteInfo)                  //Generate PX2
	TestPXData("PX2", siteInfo, sensor, &emptyData, client) //Send PX2
	sensor = GetPX3(&emptyData, siteInfo)                   //Generate PX3
	TestPXData("PX3", siteInfo, sensor, &emptyData, client) //Send PX3
	sensor = GetEvent(genPX297EventPayload, siteInfo, &emptyData)
	TestPXData("PX297", siteInfo, sensor, &emptyData, client)
	sensor = GetEvent(genPX203Payload, siteInfo, &emptyData)
	TestPXData("PX203", siteInfo, sensor, &emptyData, client)

	return TestCookie(emptyData.PX3, &emptyData, siteInfo) //Test generated cookie
}

func TestSiteCaptcha(apiKey string, siteInfo *SiteData) {
	client, _ := SetProxy("")
	//logf(fmt.Sprintf("Testing Gen for %s", strings.Title(site)))
	emptyData := PXDataStruct{}
	sensor := GetPX2(&emptyData, siteInfo)                   //Generate PX2
	TestPXData("PX2", siteInfo, sensor, &emptyData, client)  //Send PX2
	sensor2 := GetPX3(&emptyData, siteInfo)                  //Generate PX3
	TestPXData("PX3", siteInfo, sensor2, &emptyData, client) //Send PX3
	captcha, _ := GetcaptchaSolution(apiKey)
	emptyData.Grecaptcha = captcha
	sensor3 := GetCaptcha(&emptyData, siteInfo) //Generate Captcha Payload
	TestPXData("captcha", siteInfo, sensor3, &emptyData, client)
	sensor = GetEvent(genPX297EventPayload, siteInfo, &emptyData)
	TestPXData("PX297", siteInfo, sensor, &emptyData, client)
	sensor = GetEvent(genPX203Payload, siteInfo, &emptyData)
	TestPXData("PX203", siteInfo, sensor, &emptyData, client)
	TestCookie(emptyData.PX3, &emptyData, siteInfo) //Test generated cookie
}

func GetcaptchaSolution(apiKey string) (string, error) {
	client := &http.Client{}
	type taskStruct struct {
		Type       string `json:"type"`
		WebsiteURL string `json:"websiteURL"`
		WebsiteKey string `json:"websiteKey"`
		UserAgent  string `json:"userAgent"`
	}
	type payloadStruct struct {
		ClientKey string     `json:"clientKey"`
		Task      taskStruct `json:"task"`
	}

	type captchaResponse struct {
		ErrorID          int    `json:"errorId"`
		ErrorCode        string `json:"errorCode"`
		ErrorDescription string `json:"errorDescription"`
		TaskID           int    `json:"taskId"`
	}

	type taskPollStruct struct {
		ClientKey string `json:"clientKey"`
		TaskID    int    `json:"taskId"`
	}

	type solutionStruct struct {
		GRecaptchaResponse string `json:"gRecaptchaResponse"`
	}

	type taskPollResponseStruct struct {
		ErrorID   int            `json:"errorId"`
		ErrorCode string         `json:"errorCode"`
		Status    string         `json:"status"`
		Solution  solutionStruct `json:"solution"`
	}

	task := taskStruct{
		Type:       "NoCaptchaTaskProxyless",
		WebsiteURL: "https://geo.captcha-delivery.com",
		WebsiteKey: "6Lc8-RIaAAAAAPWSm2FVTyBg-Zkz2UjsWWfrkgYN",
		UserAgent:  "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.72 Safari/537.36",
	}

	payload := payloadStruct{
		ClientKey: apiKey,
		Task:      task,
	}

	jsonPayload, _ := json.Marshal(payload)
	url := "https://api.capmonster.cloud/createTask"
	resp, err := sendCaptchaRequest(url, "POST", jsonPayload, client)
	if err != nil {
		logerr("Error getting captcha task")
		return "", err
	}
	defer resp.Body.Close()
	Body, _ := ioutil.ReadAll(resp.Body)
	response := captchaResponse{}
	err = json.Unmarshal(Body, &response)

	if err != nil {
		logerr("Error getting captcha task")
		return "", err
	}
	if response.ErrorID != 0 {
		logerr("Error getting captcha task")
		return "", err
	}
	//logf(fmt.Sprintf("Started Captcha Task with ID: %d", response.TaskID))
	taskPollPayload := taskPollStruct{
		TaskID:    response.TaskID,
		ClientKey: apiKey,
	}
	taskPollJSON, _ := json.Marshal(taskPollPayload)
	taskPollSolution := solutionStruct{
		GRecaptchaResponse: "",
	}
	taskPollResponse := taskPollResponseStruct{}
	url = "https://api.capmonster.cloud/getTaskResult"
	for taskPollResponse.Status != "ready" {
		taskPollResponse = taskPollResponseStruct{Status: "not ready", Solution: taskPollSolution}
		time.Sleep(1 * time.Second)
		resp, err := sendCaptchaRequest(url, "POST", taskPollJSON, client)

		if err != nil {
			logerr("Error getting captcha task")
			return "", err
		}
		defer resp.Body.Close()
		Body, err := ioutil.ReadAll(resp.Body)
		defer resp.Body.Close()
		if err != nil {
			logerr("Error getting captcha task")
			return "", err
		}

		_ = json.Unmarshal(Body, &taskPollResponse)
		if taskPollResponse.ErrorID != 0 {
			logerr("Error getting captcha task")
			return "", err
		}

	}
	return taskPollResponse.Solution.GRecaptchaResponse, nil
	//logsuccess(fmt.Sprintf("Got captcha response: %s".universal.RecaptchaResponse))
}

func genPX2Payload(data *PXDataStruct, siteInfo *SiteData) (string, string) {
	SiteInfo := siteInfo
	var UserAgent string
	if data.UserAgent == "" {
		UserAgent = UserAgentList[rand.Intn(len(UserAgentList))]
		data.UserAgent = UserAgent
	} else {
		UserAgent = data.UserAgent
	}

	type PX2 struct {
		PX96   string `json:"PX96"`            //Link
		PX63   string `json:"PX63"`            //Platform (Win32)
		PX191  int    `json:"PX191"`           //window.self === window.top (0)
		PX850  int    `json:"PX850"`           //# of req
		PX851  int    `json:"PX851"`           //# performance.now() <-- time on page random(200,800)
		PX1008 int    `json:"PX1008"`          //3600
		PX1055 int64  `json:"PX1055"`          //timestamp - random(1,5)
		PX1056 int64  `json:"PX1056"`          //timestamp
		PX1038 string `json:"PX1038"`          //Generated UUIDV1
		PX371  bool   `json:"PX371"`           //Always true
		PX250  string `json:"PX250,omitempty"` //Always PX557
		PX708  string `json:"PX708,omitempty"` //c
	}
	var gennedUUID string
	if data.UUID == "" {
		uuidObj, _ := uuid.NewUUID()
		gennedUUID = uuidObj.String()
		data.UUID = gennedUUID
	} else {
		gennedUUID = data.UUID
	}

	px2BasePayload := PX2{
		PX96:   SiteInfo.URL,
		PX63:   "Win32",
		PX191:  0,
		PX850:  0,
		PX851:  randomNumber(200, 800),
		PX1008: 3600,
		PX1055: (time.Now().UnixNano() / 1e6) - int64(randomNumber(1, 5)),
		PX1056: (time.Now().UnixNano() / 1e6),
		PX1038: gennedUUID,
		PX371:  true,
	}

	px2Payload := []PXStruct{
		PXStruct{
			T: "PX2",
			D: px2BasePayload,
		},
	}
	strPayload, _ := JSONMarshal(px2Payload)
	return string(strPayload), gennedUUID

}
func parsePXResponse(strBody string, PXResponse *PXDataStruct) {

	sidSplit := strings.Split(strBody, "sid|")
	vidSplit := strings.Split(strBody, "vid|")
	csSplit := strings.Split(strBody, `"cs|`)
	//pxdeSplit := strings.Split(strBody, "en|_pxde|330|")
	px3Split := strings.Split(strBody, "bake|_px3|330|")
	StsSplit := strings.Split(strBody, "sts|")
	ClsSplit := strings.Split(strBody, "cls|")
	DRCSplit := strings.Split(strBody, "drc|")
	WCSSplit := strings.Split(strBody, "wcs|")
	//if len(sidSplit) < 2 || len(vidSplit) < 2 || len(csSplit) < 2 || len(pxdeSplit) < 2 {
	if !(len(sidSplit) < 2 || len(vidSplit) < 2) {
		PXResponse.SID = strings.Split(sidSplit[1], `","`)[0]
		PXResponse.VID = strings.Split(vidSplit[1], `|`)[0]
	}

	/*PXResponse.CS = strings.Split(csSplit[1], `","`)[0]
	PXResponse.PXDE = strings.Split(pxdeSplit[1], `|`)[0]*/
	if len(px3Split) > 1 {
		PXResponse.PX3 = strings.Split(px3Split[1], `|true|`)[0]
	}

	if len(StsSplit) > 1 {
		PXResponse.STS = strings.Split(StsSplit[1], `","`)[0]
	}
	if len(ClsSplit) > 1 {
		cls := strings.Split(ClsSplit[1], `","`)[0]
		clsValues := strings.Split(cls, "|")
		if len(clsValues) > 1 {
			PXResponse.CLS1 = clsValues[0]
			PXResponse.CLS2 = clsValues[1]
		}

	}

	if len(DRCSplit) > 1 {
		PXResponse.DRC = strings.Split(DRCSplit[1], `","`)[0]
	}

	if len(WCSSplit) > 1 {
		PXResponse.WCS = strings.Split(WCSSplit[1], `","`)[0]
	}
	if len(csSplit) > 1 {
		PXResponse.CS = strings.Split(csSplit[1], `","`)[0]
	}

}
func genPX3Payload(data *PXDataStruct) string {
	type PX3 struct {
		PX234    bool     `json:"PX234"`      // !! window.spawn
		PX235    bool     `json:"PX235"`      // !! window.emit
		PX151    bool     `json:"PX151"`      // window.hasOwnProperty("webdriver") || !!window["webdriver"] || "true" === document.getElementsByTagName("html")[0].getAttribute("webdriver")
		PX239    bool     `json:"PX239"`      // !!window._Selenium_IDE_Recorder
		PX240    bool     `json:"PX240"`      // !!document.__webdriver_script_fn
		PX152    bool     `json:"PX152"`      // !!window.domAutomation || !!window.domAutomationController
		PX153    bool     `json:"PX153"`      // !!window._phantom || !!window.callPhantom
		PX314    bool     `json:"PX314"`      // !!window.geb
		PX192    bool     `json:"PX192"`      // !!window.awesomium
		PX196    bool     `json:"PX196"`      // Wt(window.RunPerfTest)
		PX207    bool     `json:"PX207"`      // !!window.fmget_targets
		PX251    bool     `json:"PX251"`      // !!window.__nightmare
		PX982    int64    `json:"PX982"`      // sts value from px2
		PX983    string   `json:"PX983"`      // cls[0] from px2
		WeirdVal string   `json:"WeirdValue"` // generated from rn function
		PX986    string   `json:"PX986"`      // cls[1] from px2
		PX985    int64    `json:"PX985"`      // parseInt(drc) from px2
		PX1033   string   `json:"PX1033"`     // rn(cn(a)) <-- Browser fingerprint
		PX1019   string   `json:"PX1019"`     // pa(window, c) <--- Browser fingerprint
		PX1020   string   `json:"PX1020"`     // pa(window.document, u) <--- Browser fingerprint
		PX1021   string   `json:"PX1021"`     // pa(window.navigator, f) <--- Browser fingerprint
		PX1022   string   `json:"PX1022"`     // pa(window.location, g) <--- Browser fingerprint
		PX1035   bool     `json:"PX1035"`     // var n = -1; navigator["webdriver"] || navigator.hasOwnProperty("webdriver") || (navigator["webdriver"] = 1, n = 1 !== navigator["webdriver"], delete navigator["webdriver"]), n
		PX1139   bool     `json:"PX1139"`     // var n = -1; navigator.plugins && (navigator.plugins["refresh"] = 1, n = 1 !== navigator.plugins["refresh"], delete navigator.plugins["refresh"]), n
		PX1025   bool     `json:"PX1025"`     // o = dn(window, "navigator") && !!o["value"]
		PX359    string   `json:"PX359"`      // $(uuid, navigator.userAgent)
		PX943    string   `json:"PX943"`      // wcs from px2
		PX357    string   `json:"PX357"`      // $(vid, navigator.userAgent)
		PX358    string   `json:"PX358"`      // $(sid, navigator.userAgent)
		PX229    int      `json:"PX229"`      // window.screen.colorDepth
		PX230    int      `json:"PX230"`      // window.screen.pixelDepth
		PX91     int      `json:"PX91"`       // window.screen.width
		PX92     int      `json:"PX92"`       // window.screen.height
		PX269    int      `json:"PX269"`      // window.screen.availWidth
		PX270    int      `json:"PX270"`      // window.screen.availHeight
		PX93     string   `json:"PX93"`       // window.screen.width + "X" + window.screen.height
		PX185    int      `json:"PX185"`      // window.innerHeight
		PX186    int      `json:"PX186"`      // window.innerWidth
		PX187    int      `json:"PX187"`      // window.scrollX
		PX188    int      `json:"PX188"`      // window.scrollY
		PX95     bool     `json:"PX95"`       // !(0 === window.outerWidth && 0 === window.outerHeight)
		PX400    int      `json:"PX400"`      // ta() --> window["fetch"].length + window.performance.toJSON.length +  document['createElement'].length
		PX404    string   `json:"PX404"`      // na() --> Browser fingerprint
		PX90     []string `json:"PX90"`       // Object.keys(window.chrome)
		PX190    string   `json:"PX190"`      // window.chrome && window.chrome.runtime && window.chrome.runtime.id || ""
		PX552    string   `json:"PX552"`      // navigator['webdriver'].toString()
		PX399    string   `json:"PX399"`      // navigator['webdriver'].toString()
		PX549    int      `json:"PX549"`      // "webdriver" in navigator ? 1 : 0
		PX411    int      `json:"PX411"`      // "webdriver" in navigator ? 1 : 0
		PX405    bool     `json:"PX405"`      // !!window.caches
		PX547    bool     `json:"PX547"`      // !!window.caches
		PX134    bool     `json:"PX134"`      // !!navigator.plugins && ("[object PluginArray]" === ((void 0) = "function" == typeof navigator.plugins.toString ? navigator.plugins.toString() : navigator.plugins.constructor && "function" == typeof navigator.plugins.constructor.toString ? navigator.plugins.constructor.toString() : EC(navigator.plugins)) || "[object MSPluginsCollection]" === (void 0) || "[object HTMLPluginsCollection]" === (void 0)
		PX89     bool     `json:"PX89"`       // !!navigator.plugins && ("[object PluginArray]" === ((void 0) = "function" == typeof navigator.plugins.toString ? navigator.plugins.toString() : navigator.plugins.constructor && "function" == typeof navigator.plugins.constructor.toString ? navigator.plugins.constructor.toString() : EC(navigator.plugins)) || "[object MSPluginsCollection]" === (void 0) || "[object HTMLPluginsCollection]" === (void 0)
		PX170    int      `json:"PX170"`      // navigator.plugins.length
		PX85     []string `json:"PX85"`       // Ra() <-- list of all navigator.plugin names
		PX59     string   `json:"PX59"`       // navigator.userAgent
		PX61     string   `json:"PX61"`       // navigator.language
		PX313    []string `json:"PX313"`      // navigator.languages
		PX63     string   `json:"PX63"`       // navigator.platform
		PX86     bool     `json:"PX86"`       // !!(navigator.doNotTrack || null === navigator.doNotTrack || navigator.msDoNotTrack || window.doNotTrack
		PX154    int      `json:"PX154"`      // (new Date).getTimezoneOffset()
		PX133    bool     `json:"PX133"`      // var t = navigator.mimeTypes && navigator.mimeTypes.toString(); return "[object MimeTypeArray]" === t || /MSMimeTypesCollection/i.test(t)
		PX88     bool     `json:"PX88"`       // var t = navigator.mimeTypes && navigator.mimeTypes.toString(); return "[object MimeTypeArray]" === t || /MSMimeTypesCollection/i.test(t)
		PX169    int      `json:"PX169"`      // navigator.mimeTypes && navigator.mimeTypes.length || -1
		PX62     string   `json:"PX62"`       // navigator.product
		PX69     string   `json:"PX69"`       // navigator.productSub
		PX64     string   `json:"PX64"`       // navigator.appVersion
		PX65     string   `json:"PX65"`       // navigator.appName
		PX66     string   `json:"PX66"`       // navigator.appCodeName
		PX1144   bool     `json:"PX1144"`     // navigator.permissions && navigator.permissions.query && "query" === navigator.permissions.query.name
		PX60     bool     `json:"PX60"`       // "onLine" in navigator && !0 === navigator.onLine
		PX87     bool     `json:"PX87"`       // navigator.geolocation + "" == "[object Geolocation]"
		PX821    int      `json:"PX821"`      // window.performance.memory.jsHeapSizeLimit
		PX822    int      `json:"PX822"`      // window.performance.memory.totalJSHeapSize
		PX823    int      `json:"PX823"`      // window.performance.memory.usedJSHeapSize
		PX147    bool     `json:"PX147"`      // !!window.ActiveXObject
		PX155    string   `json:"PX155"`      // window.Date()
		PX236    bool     `json:"PX236"`      // !!window.Buffer
		PX194    bool     `json:"PX194"`      // !!window.v8Locale
		PX195    bool     `json:"PX195"`      // !!navigator.sendBeacon
		PX237    int      `json:"PX237"`      // "number" == typeof navigator.maxTouchPoints ? navigator.maxTouchPoints : "number" == typeof navigator.msMaxTouchPoints ? navigator.msMaxTouchPoints : void 0
		PX238    string   `json:"PX238"`      // navigator.msDoNotTrack || "missing"
		PX208    string   `json:"PX208"`      // document.visibilityState
		PX218    int      `json:"PX218"`      // +document.documentMode || 0
		PX231    int      `json:"PX231"`      // +window.outerHeight || 0
		PX232    int      `json:"PX232"`      // +window.outerWidth || 0
		PX254    bool     `json:"PX254"`      // !!window.showModalDialog
		PX295    bool     `json:"PX295"`      // try{document.createEvent("touchEvent")}catch(t){return !1}
		PX268    bool     `json:"PX268"`      // window.hasOwnProperty("ontouchstart") || !!window.ontouchstart
		PX166    bool     `json:"PX166"`      // Wt(window.setTimeout) <-- checks if setTimeout is function
		PX138    bool     `json:"PX138"`      // Wt(window.openDatabase)
		PX143    bool     `json:"PX143"`      // Wt(window.BatteryManager)
		PX1142   int      `json:"PX1142"`     // o.cssFromResourceApi (document.styleSheets)
		PX1143   int      `json:"PX1143"`     // o.imgFromResourceApi (document.styleSheets)
		PX1146   int      `json:"PX1146"`     // o.fontFromResourceApi (document.styleSheets)
		PX1147   int      `json:"PX1147"`     // o.cssFromStyleSheets (document.styleSheets)
		PX714    string   `json:"PX714"`      // ma(window.console.log)
		PX715    string   `json:"PX715"`      // ma(Object.getOwnPropertyDescriptor(HTMLDocument.prototype, "cookie").get)
		PX724    string   `json:"PX724"`      // ma(Object.prototype.toString
		PX725    string   `json:"PX725"`      // ma(navigator.toString)
		PX729    string   `json:"PX729"`      //t = Object.getOwnPropertyDescriptor(Object.getPrototypeOf(navigator), TC);
		PX443    bool     `json:"PX443"`      // !!window.isSecureContext
		PX466    bool     `json:"PX466"`      // !!window.Worklet
		PX467    bool     `json:"PX467"`      // !!window.AudioWorklet
		PX468    bool     `json:"PX468"`      // !window.AudioWorkletNode
		PX191    int      `json:"PX191"`      // window.self === window.top ? 0 : 1
		PX94     int      `json:"PX94"`       // window.history && "number" == typeof window.history.length && window.history.length || -1
		PX120    []string `json:"PX120"`      // list of non-null document.location.ancestorOrigins
		PX141    bool     `json:"PX141"`      // window.hasOwnProperty("onorientationchange")
		PX96     string   `json:"PX96"`       // window.location && window.location.href || ""
		PX55     string   `json:"PX55"`       // document.referrer ? encodeURIComponent(document.referrer) : ""
		PX1065   int      `json:"PX1065"`     // # of px3 requests
		PX850    int      `json:"PX850"`      // # of reqs
		PX851    int      `json:"PX851"`      // Math.round(window.performance.now()
		PX1054   int      `json:"PX1054"`     // timestamp 2
		PX1008   int      `json:"PX1008"`     // 3600 (static)
		PX1055   int      `json:"PX1055"`     // timestamp 1
		PX1056   int      `json:"PX1056"`     // timestamp 3
		PX1038   string   `json:"PX1038"`     // uuid
		PX371    bool     `json:"PX371"`      // true (static)
	}

	var UserAgent string
	//Browser Values
	if data.UserAgent != "" {
		UserAgent = data.UserAgent
	} else {
		UserAgent = UserAgentList[rand.Intn(len(UserAgentList))]
	}

	UserAgentVersion := strings.Split(UserAgent, "Mozilla/")[1]
	//Timestamps
	PX1056 := int(time.Now().UnixNano() / 1e5)
	PX1054 := PX1056 - randomNumber(400, 800)
	PX1055 := PX1054 - randomNumber(100, 250)
	PX851 := PX1056 - PX1055 + randomNumber(50, 150)
	currentDate := time.Now()
	formattedDate := currentDate.Format("Mon Jan 02 2006 15:04:05 GMT-0700 (Eastern Daylight Time)")

	//Parse Values from PX2
	intSTS, _ := strconv.ParseInt(data.STS, 10, 64)
	CLS1 := data.CLS1
	CLS2 := data.CLS2
	DRC, _ := strconv.ParseInt(data.DRC, 10, 64)
	UUID := data.UUID
	SID := data.SID
	VID := data.VID
	WCS := data.WCS

	//Dynamic Browser Values
	HeapSizeLimit := 4294705152 //static for the most part
	TotalJSHeapSize := randomNumber(11056023, 44443137)
	UsedJSHeapSize := TotalJSHeapSize - randomNumber(4065828, 12634832)

	//"Fixed" Browser Values
	ScreenSizes := [][]int{{2560, 1440}, {2304, 1728}, {3440, 1440}, {2280, 1800}, {2880, 1600}, {3200, 2048}, {4500, 3000}, {2560, 1080}, {1920, 1200}, {1920, 1080}, {1600, 1200}}
	ScreenSize := ScreenSizes[rand.Intn(len(ScreenSizes))]
	Width := ScreenSize[0]
	Height := ScreenSize[1]
	InnerWidth := (Width / 2) + randomNumber(100, 500)
	InnerHeight := (Height / 2) + randomNumber(100, 500)
	PX231 := randomNumber(1100, 1400)
	PX232 := PX231 + randomNumber(100, 250)
	ColorBitsList := []int{16, 24, 32}
	ColorBits := ColorBitsList[rand.Intn(len(ColorBitsList))]
	//Possible Page Values
	PX400Vals := []int{281, 285, 585, 111, 123}
	PX400 := PX400Vals[rand.Intn(len(PX400Vals))]
	PX400 = 111 // Can cause invalids, seems to be dependent on site

	//Weird Value
	StSInt, _ := strconv.ParseInt(data.STS, 10, 64)
	StSInt = (StSInt % 10) + 2
	WeirdValKey := rn(CLS1, fmt.Sprintf("%d", StSInt))
	WeirdValValue := rn(CLS2, fmt.Sprintf("%d", StSInt))
	px3BasePayload := PX3{
		PX234:    false,
		PX235:    false,
		PX151:    false,
		PX239:    false,
		PX240:    false,
		PX152:    false,
		PX153:    false,
		PX314:    false,
		PX192:    false,
		PX196:    false,
		PX207:    false,
		PX251:    false,
		PX982:    intSTS,
		PX983:    CLS1,
		WeirdVal: WeirdValValue,
		PX986:    CLS2,
		PX985:    DRC,
		PX1033:   "e0eaf10e",
		PX1019:   "d1917ca4",
		PX1020:   "7766a52d",
		PX1021:   "180dd7e3",
		PX1022:   "6a90378d",
		PX1035:   true,
		PX1139:   false,
		PX1025:   false,
		PX359:    H(UUID, UserAgent),
		PX943:    WCS,
		PX357:    H(VID, UserAgent),
		PX358:    H(SID, UserAgent),
		PX229:    ColorBits,
		PX230:    ColorBits,
		PX91:     Width,
		PX92:     Height,
		PX269:    Width,
		PX270:    Height,
		PX93:     fmt.Sprintf("%dx%d", Width, Height),
		PX185:    InnerHeight,
		PX186:    InnerWidth,
		PX187:    0,
		PX188:    0,
		PX95:     true,
		PX400:    PX400,
		PX404:    "144|54|54|180|68",
		PX90: []string{
			"loadTimes",
			"csi",
			"app",
			"runtime",
		},
		PX190: "",
		PX552: "false",
		PX399: "false",
		PX549: 1,
		PX411: 1,
		PX405: true,
		PX547: true,
		PX134: true,
		PX89:  true,
		PX170: 3,
		PX85: []string{
			"Chrome PDF Plugin",
			"Chrome PDF Viewer",
			"Native Client",
		},
		PX59: UserAgent,
		PX61: "en-US",
		PX313: []string{
			"en-US",
		},
		PX63:   "Win32",
		PX86:   true,
		PX154:  240,
		PX133:  true,
		PX88:   true,
		PX169:  4,
		PX62:   "Gecko",
		PX69:   "20030107",
		PX64:   UserAgentVersion,
		PX65:   "Netscape",
		PX66:   "Mozilla",
		PX1144: true,
		PX60:   true,
		PX87:   true,
		PX821:  HeapSizeLimit,
		PX822:  TotalJSHeapSize,
		PX823:  UsedJSHeapSize,
		PX147:  false,
		PX155:  formattedDate,
		PX236:  false,
		PX194:  false,
		PX195:  true,
		PX237:  0,
		PX238:  "missing",
		PX208:  "visible",
		PX218:  0,
		PX231:  PX231,
		PX232:  PX232,
		PX254:  false,
		PX295:  false,
		PX268:  false,
		PX166:  true,
		PX138:  true,
		PX143:  true,
		PX1142: randomNumber(5, 8),
		PX1143: randomNumber(30, 40), //39,
		PX1146: 0,
		PX1147: 2,
		PX714:  "64556c77",
		PX715:  "",
		PX724:  "10207b2f",
		PX725:  "10207b2f",
		PX729:  "90e65465",
		PX443:  true,
		PX466:  true,
		PX467:  true,
		PX468:  true,
		PX191:  0,
		PX94:   2,
		PX120:  []string{},
		PX141:  false,
		PX96:   "https://www.walmart.com/",
		PX55:   "",
		PX1065: 1,
		PX850:  1,
		PX851:  PX851,
		PX1054: PX1054,
		PX1008: 3600,
		PX1055: PX1055,
		PX1056: PX1056,
		PX1038: UUID,
		PX371:  true,
	}
	px3Payload := []PXStruct{
		PXStruct{
			T: "PX3",
			D: px3BasePayload,
		},
	}

	bytePayload, _ := JSONMarshal(px3Payload)
	strPayload := string(bytePayload)
	strPayload = strings.Replace(strPayload, "WeirdValue", WeirdValKey, 1)
	//logf(strPayload)
	return strPayload

}
func JSONMarshal(t interface{}) ([]byte, error) {
	buffer := &bytes.Buffer{}
	encoder := json.NewEncoder(buffer)
	encoder.SetEscapeHTML(false)
	err := encoder.Encode(t)
	return buffer.Bytes(), err
}

func GetPX2(data *PXDataStruct, siteInfo *SiteData) SensorDataStruct {
	payload, uuid := genPX2Payload(data, siteInfo)
	pc := pxutils.CreatePC(payload, fmt.Sprintf("%s:%s:%s", uuid, siteInfo.Tag, siteInfo.Ft))
	encodedPayload := pxutils.EncodePayload(payload, 50)
	sensorData := SensorDataStruct{
		Payload: encodedPayload,
		AppID:   siteInfo.AppID,
		Tag:     siteInfo.Tag,
		Uuid:    uuid,
		Ft:      siteInfo.Ft,
		Seq:     0,
		En:      "NTA",
		Pc:      pc,
		Rsc:     1,
	}

	return sensorData

}
func GetCaptcha(data *PXDataStruct, siteInfo *SiteData) SensorDataStruct {
	SiteInfo := siteInfo
	payload := genCaptchaPayload(data.Grecaptcha, data, siteInfo)
	pc := pxutils.CreatePC(payload, fmt.Sprintf("%s:%s:%s", data.UUID, SiteInfo.Tag, SiteInfo.Ft))
	payload = pxutils.EncodePayload(payload, 50)
	SensorData := SensorDataStruct{
		Payload: payload,
		AppID:   SiteInfo.AppID,
		Tag:     SiteInfo.Tag,
		Uuid:    data.UUID,
		Ft:      SiteInfo.Ft,
		Seq:     1,
		En:      "NTA",
		Pc:      pc,
		Sid:     data.SID,
		Vid:     data.VID,
		Rsc:     2,
	}
	SensorDataJSON, _ := json.Marshal(SensorData)
	logf(string(SensorDataJSON))
	return SensorData

	//return (string(SensorDataJSON))
}
func genCaptchaPayload(captcha string, data *PXDataStruct, siteInfo *SiteData) string {

	// Previous Values
	RecapToken := data.Grecaptcha
	UUID := data.UUID
	VID := data.VID
	if data.UserAgent == "" {
		//UserAgent = UserAgentList[rand.Intn(len(UserAgentList))]
		data.UserAgent = UserAgentList[rand.Intn(len(UserAgentList))]
	}
	type PX297 struct {
		PX38   string `json:"PX38"`   // event type
		PX70   int    `json:"PX70"`   //random value between 2-3k
		PX157  string `json:"PX157"`  //statically "true"
		PX72   string `json:"PX72"`   // element event was done on
		PX34   string `json:"PX34"`   // static error message
		PX78   int    `json:"PX78"`   // Mouse x
		PX79   int    `json:"PX79"`   // Mouse y
		PX850  int    `json:"PX850"`  // num req including PX3
		PX851  int    `json:"PX851"`  // window.performance.now()
		PX1056 int64  `json:"PX1056"` // timestamp
		PX1038 string `json:"PX1038"` // uuid
		PX371  bool   `json:"PX371"`  // statically true
		PX250  string `json:"PX250"`  // static at "PX557",
		PX708  string `json:"PX708"`  // static at "c"
		PX96   string `json:"PX96"`   // page url
	}

	PX851 := randomNumber(18000, 25000)
	PX70 := PX851 - randomNumber(300, 700)
	PX79 := PX851 - PX70 + randomNumber(15, 75)
	PX1056 := (time.Now().UnixNano() / 1e6)
	PX297Data := PX297{
		PX38:   "mouseover",
		PX70:   PX70,
		PX157:  "true",
		PX72:   "#sign-in-widget",
		PX34:   "TypeError: Cannot read property '0' of null\n    at Mt (https://www.walmart.com/px/PXu6b0qd2S/init.js:2:14772)\n    at HTMLBodyElement.ke (https://www.walmart.com/px/PXu6b0qd2S/init.js:2:29669)",
		PX78:   randomNumber(2500, 3500),
		PX79:   PX79,
		PX850:  5,
		PX851:  PX851,
		PX1056: PX1056,
		PX1038: data.UUID,
		PX371:  true,
		PX250:  "PX557",
		PX708:  "c",
		PX96:   fmt.Sprintf("https://www.%s.com/blocked?url=Lw==&uuid=%s&vid=%s&g=a", siteInfo.SiteHostName, UUID, VID),
	}
	type PX761 struct {
		PX70   int    `json:"PX70"` // Start of captcha performance.now()
		PX34   string `json:"PX34"` // Static error message
		PX1129 bool   `json:"PX1129"`
		PX1130 bool   `json:"PX1130"`
		PX610  bool   `json:"PX610"`
		PX754  bool   `json:"PX754"`
		PX755  string `json:"PX755"`  // Captcha Token
		PX756  string `json:"PX756"`  // Type of captcha
		PX757  string `json:"PX757"`  // Site URL
		PX1151 string `json:"PX1151"` // Static Value
		PX850  int    `json:"PX850"`  // #Reqs including PX3
		PX851  int    `json:"PX851"`  // Window.Performance.now()
		PX1056 int64  `json:"PX1056"` // Timestamp
		PX1038 string `json:"PX1038"` // uuid
		PX371  bool   `json:"PX371"`
		PX250  string `json:"PX250"` // Static at "PX557"
		PX708  string `json:"PX708"` // Static at "c"
	}

	// Timestamp values
	PX851 = randomNumber(20000, 40000)
	PX70 = PX851 - randomNumber(500, 1500)

	PX761Data := PX761{
		PX70:   PX70,
		PX34:   "TypeError: Cannot read property '0' of null\n    at Mt (https://www.walmart.com/px/PXu6b0qd2S/init.js:2:14772)\n    at se (https://www.walmart.com/px/PXu6b0qd2S/init.js:2:28206)\n    at Object.$n [as PX763] (https://www.walmart.com/px/PXu6b0qd2S/init.js:2:26889)\n    at o (https://www.walmart.com/px/PXu6b0qd2S/captcha/captcha.js?a=c&m=0&g=a:3:62549)\n    at window.<computed> (https://www.walmart.com/px/PXu6b0qd2S/captcha/captcha.js?a=c&m=0&g=a:3:64890)\n    at Rn.window.<computed>.window.<computed> (https://www.walmart.com/px/PXu6b0qd2S/init.js:2:24243)\n    at ML.C.ML.R (https://www.gstatic.com/recaptcha/releases/f-bnnOuahiYKuei7dmAd3kgv/recaptcha__en.js:678:222)\n    at w.X (https://www.gstatic.com/recaptcha/releases/f-bnnOuahiYKuei7dmAd3kgv/recaptcha__en.js:108:122)\n    at new Promise (<anonymous>)\n    at $o.X (https://www.gstatic.com/recaptcha/releases/f-bnnOuahiYKuei7dmAd3kgv/recaptcha__en.js:108:94)",
		PX1129: true,
		PX1130: false,
		PX610:  true,
		PX754:  false,
		PX755:  RecapToken,
		PX756:  "reCaptcha",
		PX757:  fmt.Sprintf("www.%s.com", siteInfo.SiteHostName),
		PX1151: "4YCJ4YGQ4YCa4YCG4YCf4YCe4YGQ4YGI4YGD4YGe4YGQ4YCa4YCX4YCT4YCW4YGQ4YGI4YGD4YGe4YGQ4YCG4YCb4YCG4YCe4YCX4YGQ4YGI4YGD4YGe4YGQ4YCf4YCX4YCG4YCT4YGQ4YGI4YGD4YGe4YGQ4YCB4YCG4YCL4YCe4YCX4YGQ4YGI4YGD4YGe4YGQ4YCB4YCR4YCA4YCb4YCC4YCG4YGQ4YGI4YGF4YGe4YGQ4YCQ4YCd4YCW4YCL4YGQ4YGI4YGD4YGe4YGQ4YCW4YCb4YCE4YGQ4YGI4YGD4YGF4YGe4YGQ4YCT4YGQ4YGI4YGE4YGe4YGQ4YCB4YCC4YCT4YCc4YGQ4YGI4YGH4YGe4YGQ4YCa4YGD4YGQ4YGI4YGD4YGe4YGQ4YCC4YGQ4YGI4YGA4YGe4YGQ4YCb4YCU4YCA4YCT4YCf4YCX4YGQ4YGI4YGB4YGe4YGQ4YCG4YCX4YCK4YCG4YCT4YCA4YCX4YCT4YGQ4YGI4YGD4YGe4YGQ4YCa4YCA4YGQ4YGI4YGD4YCP",
		PX850:  5,
		PX851:  PX851,
		PX1056: PX1056,
		PX1038: UUID,
		PX371:  true,
		PX250:  "PX557",
		PX708:  "c",
	}

	CaptchaPayload := []PXStruct{
		PXStruct{
			T: "PX297",
			D: PX297Data,
		},
		PXStruct{
			T: "PX761",
			D: PX761Data,
		},
	}
	bytePayload, _ := JSONMarshal(CaptchaPayload)
	logf(string(bytePayload))
	return string(bytePayload)

}

func GetPX3(data *PXDataStruct, siteInfo *SiteData) SensorDataStruct {
	payload := genPX3Payload(data)
	pc := pxutils.CreatePC(payload, fmt.Sprintf("%s:%s:%s", data.UUID, siteInfo.Tag, siteInfo.Ft))
	encodedPayload := pxutils.EncodePayload(payload, 50)

	//logf(encodedPayload)
	sensorData := SensorDataStruct{
		Payload: encodedPayload,
		AppID:   siteInfo.AppID,
		Tag:     siteInfo.Tag,
		Uuid:    data.UUID,
		Ft:      siteInfo.Ft,
		Seq:     1,
		En:      "NTA",
		Cs:      data.CS,
		Pc:      pc,
		Sid:     data.SID,
		Vid:     data.VID,
		Rsc:     2,
	}
	SensorDataJSON, _ := json.Marshal(sensorData)
	logf(string(SensorDataJSON))
	//jsonSensor, _ := JSONMarshal(sensorData)
	return sensorData

}

func GetEvent(eventPayload func(site string, data *PXDataStruct) string, siteInfo *SiteData, data *PXDataStruct) SensorDataStruct {
	payload := eventPayload(siteInfo.SiteHostName, data)
	pc := pxutils.CreatePC(payload, fmt.Sprintf("%s:%s:%s", data.UUID, siteInfo.Tag, siteInfo.Ft))
	encodedPayload := pxutils.EncodePayload(payload, 50)
	//logf(encodedPayload)
	sensorData := SensorDataStruct{
		Payload: encodedPayload,
		AppID:   siteInfo.AppID,
		Tag:     siteInfo.Tag,
		Uuid:    data.UUID,
		Ft:      siteInfo.Ft,
		Seq:     3,
		En:      "NTA",
		Cs:      data.CS,
		Pc:      pc,
		Sid:     data.SID,
		Vid:     data.VID,
		Rsc:     3,
	}
	return sensorData
}
func randomString(n int) string {
	const letterBytes = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ"
	const (
		letterIdxBits = 6                    // 6 bits to represent a letter index
		letterIdxMask = 1<<letterIdxBits - 1 // All 1-bits, as many as letterIdxBits
		letterIdxMax  = 63 / letterIdxBits   // # of letter indices fitting in 63 bits
	)

	b := make([]byte, n)
	// A rand.Int63() generates 63 random bits, enough for letterIdxMax letters!
	for i, cache, remain := n-1, rand.Int63(), letterIdxMax; i >= 0; {
		if remain == 0 {
			cache, remain = rand.Int63(), letterIdxMax
		}
		if idx := int(cache & letterIdxMask); idx < len(letterBytes) {
			b[i] = letterBytes[idx]
			i--
		}
		cache >>= letterIdxBits
		remain--
	}

	return string(b)
}
func genPX297EventPayload(site string, data *PXDataStruct) string {
	type PX297 struct {
		PX38   string `json:"PX38"`   // event type
		PX70   int    `json:"PX70"`   //random value between 2-3k
		PX157  string `json:"PX157"`  //statically "true"
		PX72   string `json:"PX72"`   // element event was done on
		PX34   string `json:"PX34"`   // static error message
		PX78   int    `json:"PX78"`   // Mouse x
		PX79   int    `json:"PX79"`   // Mouse y
		PX850  int    `json:"PX850"`  // num req including PX3
		PX851  int    `json:"PX851"`  // window.performance.now()
		PX1056 int64  `json:"PX1056"` // timestamp
		PX1038 string `json:"PX1038"` // uuid
		PX371  bool   `json:"PX371"`  // statically true
		PX96   string `json:"PX96"`   // page url
	}

	PX297Data := PX297{
		PX38:   "mouseover",
		PX70:   randomNumber(2000, 3000),
		PX157:  "true",
		PX72:   fmt.Sprintf("#%s", randomString(randomNumber(10, 25))),
		PX34:   "TypeError: Cannot read property '0' of null\n    at Mt (https://www.walmart.com/px/PXu6b0qd2S/init.js:2:14772)\n    at HTMLBodyElement.ke (https://www.walmart.com/px/PXu6b0qd2S/init.js:2:29669)",
		PX78:   randomNumber(500, 800),
		PX79:   randomNumber(200, 400),
		PX850:  2,
		PX851:  randomNumber(3000, 3500),
		PX1056: time.Now().UnixNano() / 1e5,
		PX1038: data.UUID,
		PX371:  true,
		PX96:   fmt.Sprintf("https://www.%s.com/", site),
	}
	PX297Payload := []PXStruct{
		PXStruct{
			T: "PX297",
			D: PX297Data,
		},
	}
	bytePayload, _ := json.Marshal(PX297Payload)
	return string(bytePayload)
}

func genPX203Payload(site string, data *PXDataStruct) string {
	type PX203 struct {
		PX204  int    `json:"PX204"`
		PX59   string `json:"PX59"`
		PX887  string `json:"PX887"`
		PX888  int    `json:"PX888"`
		PX839  int    `json:"PX839"`
		PX919  int    `json:"PX919"`
		PX359  string `json:"PX359"`
		PX357  string `json:"PX357"`
		PX358  string `json:"PX358"`
		PX850  int    `json:"PX850"`
		PX851  int    `json:"PX851"`
		PX1056 int64  `json:"PX1056"`
		PX1038 string `json:"PX1038"`
		PX371  bool   `json:"PX371"`
		PX96   string `json:"PX96"`
	}

	PX203Data := PX203{
		PX204:  1,
		PX59:   data.UserAgent,
		PX887:  "false",
		PX839:  0,
		PX919:  0,
		PX359:  H(data.UUID, data.UserAgent),
		PX357:  H(data.VID, data.UserAgent),
		PX358:  H(data.SID, data.UserAgent),
		PX850:  3,
		PX851:  randomNumber(5000, 8000),
		PX1056: time.Now().UnixNano() / 1e5,
		PX1038: data.UUID,
		PX371:  true,
		PX96:   fmt.Sprintf("https://www.%s.com/", site),
	}
	PX203Payload := []PXStruct{
		PXStruct{
			T: "PX203",
			D: PX203Data,
		},
	}
	bytePayload, _ := json.Marshal(PX203Payload)
	return string(bytePayload)
}

func TestPXData(step string, siteInfo *SiteData, sensor SensorDataStruct, data *PXDataStruct, client *http.Client) (PXDataStruct, error) {
	PXResponse := PXDataStruct{}
	querySensor, _ := query.Values(sensor)

	reqURL := fmt.Sprintf("https://collector-%s.px-cloud.net/api/v2/collector", siteInfo.AppID)
	if step == "captcha" {
		reqURL = fmt.Sprintf("https://collector-%s.px-cloud.net/assets/js/bundle", siteInfo.AppID)
	}
	resp, err := sendRequest(reqURL, "POST", []byte(querySensor.Encode()), data, client)
	if err != nil {
		logerr("An error occurred testing gen")
		return PXResponse, err
	}
	Body, err := ioutil.ReadAll(resp.Body)
	if err != nil {
		logerr("Error reading px2 response")
		return PXResponse, err
	}
	strBody := string(Body)
	logf(strBody)
	parsePXResponse(strBody, data)
	if err != nil {
		logerr(fmt.Sprintf("Failed %s (Bad Response)", step))
		return PXResponse, err
	}
	PXResponse.UUID = sensor.Uuid
	//logf(strBody)
	logsuccess(fmt.Sprintf("Submitted %s for %s", step, siteInfo.URL))
	//logsuccess(fmt.Sprintf("Submitted %s  \nSID:  %s\nVID:  %s\nCS:   %s \nPXDE: %s\nPX3:  %s", step, PXResponse.SID, PXResponse.VID, PXResponse.CS, PXResponse.PXDE, PXResponse.PX3))
	return PXResponse, nil
}

func randomNumber(min int, max int) int {
	return rand.Intn(max-min) + min
}

func TestCookie(px3 string, data *PXDataStruct, siteInfo *SiteData) bool {
	client := &http.Client{}
	var resp *http.Response
	var err error
	if data != nil {
		resp, err = sendTestRequest(siteInfo.URL, "GET", px3, nil, data, client)
	} else {
		resp, err = sendTestRequest(siteInfo.URL, "GET", px3, nil, &PXDataStruct{}, client)
	}

	if err != nil {
		logerr("Error testing cookie")
		return false
	}
	body, _ := ioutil.ReadAll(resp.Body)
	strBody := string(body)
	if resp.StatusCode == 200 && !strings.Contains(strBody, "px-captcha") {
		logsuccess(fmt.Sprintf("Valid Cookie for %s", strings.Title(siteInfo.SiteHostName)))
		return true
	} else {
		logerr(fmt.Sprintf("Invalid Cookie for %s", strings.Title(siteInfo.SiteHostName)))
		return false
	}
}

func SolveCookie(proxy string, emptyData PXDataStruct, siteInfo *SiteData) (*PXDataStruct, error) {
	client, err := SetProxy(proxy)
	if err != nil {
		return nil, err
	}
	//mptyData := PXDataStruct{}
	sensor := GetPX2(&emptyData, siteInfo)                  //Generate PX2
	TestPXData("PX2", siteInfo, sensor, &emptyData, client) //Send PX2
	sensor = GetPX3(&emptyData, siteInfo)                   //Generate PX3
	TestPXData("PX3", siteInfo, sensor, &emptyData, client) //Send PX3
	//sensor = GetEvent(genPX297EventPayload, site, &emptyData)
	//TestPXData("PX297", site, sensor, &emptyData, client)
	//sensor = GetEvent(genPX203Payload, site, &emptyData)
	//TestPXData("PX203", site, sensor, &emptyData, client)
	return &emptyData, nil

}
func SolveCookieCaptcha(proxy string, siteInfo *SiteData, apiKey string, emptyData PXDataStruct) (*PXDataStruct, error) {
	client, err := SetProxy(proxy)
	if err != nil {
		return nil, err
	}
	//emptyData := PXDataStruct{}
	sensor := GetPX2(&emptyData, siteInfo)                  //Generate PX2
	TestPXData("PX2", siteInfo, sensor, &emptyData, client) //Send PX2
	sensor = GetPX3(&emptyData, siteInfo)                   //Generate PX3
	TestPXData("PX3", siteInfo, sensor, &emptyData, client) //Send PX3
	sensor = GetEvent(genPX297EventPayload, siteInfo, &emptyData)
	TestPXData("PX297", siteInfo, sensor, &emptyData, client)
	sensor = GetEvent(genPX203Payload, siteInfo, &emptyData)
	TestPXData("PX203", siteInfo, sensor, &emptyData, client)
	captcha, _ := GetcaptchaSolution(apiKey)
	emptyData.Grecaptcha = captcha
	sensor = GetCaptcha(&emptyData, siteInfo)
	TestPXData("captcha", siteInfo, sensor, &emptyData, client)
	return &emptyData, nil

}

func sendTestRequest(URL string, reqType string, cookie string, jsonValue []byte, data *PXDataStruct, client *http.Client) (*http.Response, error) {
	req, _ := http.NewRequest(reqType, URL, bytes.NewBuffer(jsonValue))
	UserAgent := "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/88.0.4324.104 Safari/537.36"
	if data.UserAgent != "" {
		UserAgent = data.UserAgent
	}
	req.Header = http.Header{
		//"Host":             {"collector-pxu6b0qd2s.px-cloud.net"},
		"sec-ch-ua":        {`Google Chrome";v="89", "Chromium";v="89", ";Not A Brand";v="99"`},
		"sec-ch-ua-mobile": {"?0"},
		"user-agent":       {UserAgent},
		"content-type":     {"application/x-www-form-urlencoded"},
		"accept":           {"*/*"},
		"origin":           {"https://www.walmart.com"},
		"sec-fetch-site":   {"cross-site"},
		"sec-fetch-mode":   {"cors"},
		"sec-fetch-dest":   {"empty"},
		"accept-encoding":  {},
		"referer":          {"https://www.walmart.com/"},
		"accept-language":  {"en-US,en;q=0.9"},
		"content-length":   {},
		"cookie":           {fmt.Sprintf("_px3=%s;", cookie)},
		http.HeaderOrderKey: {
			//"Host",
			"sec-ch-ua",
			"sec-ch-ua-mobile",
			"user-agent",
			"content-type",
			"accept",
			"origin",
			"sec-fetch-site",
			"sec-fetch-mode",
			"sec-fetch-dest",
			"accept-encoding",
			"referer",
			"accept-language",
			"content-length",
		},
		http.PHeaderOrderKey: {":authority", ":method", ":path", ":scheme"},
	}
	//req.Header.Set("cookie", "_px3=62cd810b35b5746d76532c68cf7973509b3e9c2aa7f8074a0495cef1bb18ebb1:Rwwi9IV1JduNHiRGYS3N0tQuGJdWY9eGwQ0C9lIDzLxoZuflw3m/DNsAPWdeQxF+0CFnO8qd3zgLRuSJrkrTow==:1000:VQ2Rf9TvBjspuanNQ8a4NMvVK7yTasdm0qr8cun672n23FteZLXREPYr8RbBbp4HRWtwiOfks8+1Q34lnBvaPM0A7KiUkyKw0ZbM5YzaZl1MffOfJNsygg8u8Lo6JMWIZVaCRVIEpP5iBJZhlHboA1cqVE0Da9QmFmfLyEbibyE=;")
	//req.Header.Set("cookie", "_px3=402864ed5866c9c7dfb101f0c885ddeef5824049997cef7198b2ff01b5e18d31:52NYQJZaPJ3aWEQ4cJW6qbuJI+0i6K9o3h0rCXs733LJv0mBdOF2SD5FSibdaGaVATyeCo28U6XEsSlka7BnkQ==:1000:MJicuw2m3fkMso7qoHXZHWS7I73kdaVh/eOI7Moe/umL72R9cwsFEoHjqUl7wiV0elRpxLYyA6D/DJduUc5kHUnCjRMDCmUch9kVLP+6ywTWS6GQ4iqv89ayTK6m+XvMdZGV73VnYZm5pYiW8LVWxwGzpLbm13Qrh7P3S10CH5o=;")
	resp, err := client.Do(req)

	return resp, err
}
func sendRequest(URL string, reqType string, jsonValue []byte, data *PXDataStruct, client *http.Client) (*http.Response, error) {
	req, _ := http.NewRequest(reqType, URL, bytes.NewBuffer(jsonValue))
	UserAgent := "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/88.0.4324.104 Safari/537.36"
	if data.UserAgent != "" {
		UserAgent = data.UserAgent
	}
	req.Header = http.Header{
		//"Host":             {"collector-pxu6b0qd2s.px-cloud.net"},
		"sec-ch-ua":        {`Google Chrome";v="89", "Chromium";v="89", ";Not A Brand";v="99"`},
		"sec-ch-ua-mobile": {"?0"},
		"user-agent":       {UserAgent},
		"content-type":     {"application/x-www-form-urlencoded"},
		"accept":           {"*/*"},
		"origin":           {"https://www.walmart.com"},
		"sec-fetch-site":   {"cross-site"},
		"sec-fetch-mode":   {"cors"},
		"sec-fetch-dest":   {"empty"},
		"accept-encoding":  {},
		"referer":          {"https://www.walmart.com/"},
		"accept-language":  {"en-US,en;q=0.9"},
		"content-length":   {},
		http.HeaderOrderKey: {
			//"Host",
			"sec-ch-ua",
			"sec-ch-ua-mobile",
			"user-agent",
			"content-type",
			"accept",
			"origin",
			"sec-fetch-site",
			"sec-fetch-mode",
			"sec-fetch-dest",
			"accept-encoding",
			"referer",
			"accept-language",
			"content-length",
		},
		http.PHeaderOrderKey: {":authority", ":method", ":path", ":scheme"},
	}
	//req.Header.Set("cookie", "_px3=62cd810b35b5746d76532c68cf7973509b3e9c2aa7f8074a0495cef1bb18ebb1:Rwwi9IV1JduNHiRGYS3N0tQuGJdWY9eGwQ0C9lIDzLxoZuflw3m/DNsAPWdeQxF+0CFnO8qd3zgLRuSJrkrTow==:1000:VQ2Rf9TvBjspuanNQ8a4NMvVK7yTasdm0qr8cun672n23FteZLXREPYr8RbBbp4HRWtwiOfks8+1Q34lnBvaPM0A7KiUkyKw0ZbM5YzaZl1MffOfJNsygg8u8Lo6JMWIZVaCRVIEpP5iBJZhlHboA1cqVE0Da9QmFmfLyEbibyE=;")
	//req.Header.Set("cookie", "_px3=402864ed5866c9c7dfb101f0c885ddeef5824049997cef7198b2ff01b5e18d31:52NYQJZaPJ3aWEQ4cJW6qbuJI+0i6K9o3h0rCXs733LJv0mBdOF2SD5FSibdaGaVATyeCo28U6XEsSlka7BnkQ==:1000:MJicuw2m3fkMso7qoHXZHWS7I73kdaVh/eOI7Moe/umL72R9cwsFEoHjqUl7wiV0elRpxLYyA6D/DJduUc5kHUnCjRMDCmUch9kVLP+6ywTWS6GQ4iqv89ayTK6m+XvMdZGV73VnYZm5pYiW8LVWxwGzpLbm13Qrh7P3S10CH5o=;")
	resp, err := client.Do(req)

	return resp, err
}

func sendCaptchaRequest(URL string, reqType string, jsonValue []byte, client *http.Client) (*http.Response, error) {
	req, _ := http.NewRequest(reqType, URL, bytes.NewBuffer(jsonValue))
	req.Header.Set("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.89 Safari/537.36")
	req.Header.Set("Content-Type", "text/plain")
	//req.Header.Set("cookie", "_px3=402864ed5866c9c7dfb101f0c885ddeef5824049997cef7198b2ff01b5e18d31:52NYQJZaPJ3aWEQ4cJW6qbuJI+0i6K9o3h0rCXs733LJv0mBdOF2SD5FSibdaGaVATyeCo28U6XEsSlka7BnkQ==:1000:MJicuw2m3fkMso7qoHXZHWS7I73kdaVh/eOI7Moe/umL72R9cwsFEoHjqUl7wiV0elRpxLYyA6D/DJduUc5kHUnCjRMDCmUch9kVLP+6ywTWS6GQ4iqv89ayTK6m+XvMdZGV73VnYZm5pYiW8LVWxwGzpLbm13Qrh7P3S10CH5o=;")
	resp, err := client.Do(req)
	return resp, err
}

func logerr(data string) {
	errorlog.Printf("[%s] [MAIN] %s\n\n", (time.Now().Format("15:04:05.000")), data)
}
func logf(data string) {
	//tasklog.Printf("[%s] [MAIN] %s\n\n", (time.Now().Format("15:04:05.000")), data)
}

func logsuccess(data string) {
	successlog.Printf("[%s] [MAIN] %s\n\n", (time.Now().Format("15:04:05.000")), data)
}

//PX helper functions

func H(param1 string, param2 string) string {
	ctx := otto.New()
	ctx.Run(`function Q(t, n) {
		var e = void 0,
			r = W(t),
			o = [],
			i = [];
		for (o[15] = i[15] = void 0, r.length > 16 && (r = V(r, 8 * t.length)), e = 0; e < 16; e += 1) o[e] = 909522486 ^ r[e], i[e] = 1549556828 ^ r[e];
		var a = V(o.concat(W(n)), 512 + 8 * n.length);
		return j(V(i.concat(a), 640))
	}
	function B(t) {
		var n = "0123456789abcdef",
			e = "",
			r = void 0,
			o = void 0;
		for (o = 0; o < t.length; o += 1) r = t.charCodeAt(o), e += n.charAt(r >>> 4 & 15) + n.charAt(15 & r);
		return e
	}
	
	function W(t) {
		var n = void 0,
			e = [];
		for (e[(t.length >> 2) - 1] = void 0, n = 0; n < e.length; n += 1) e[n] = 0;
		for (n = 0; n < 8 * t.length; n += 8) e[n >> 5] |= (255 & t.charCodeAt(n / 8)) << n % 32;
		return e
	}
	function V(t, n) {
		t[n >> 5] |= 128 << n % 32, t[14 + (n + 64 >>> 9 << 4)] = n;
		var e = void 0,
			r = void 0,
			o = void 0,
			i = void 0,
			a = void 0,
			c = 1732584193,
			u = -271733879,
			f = -1732584194,
			g = 271733878;
		for (e = 0; e < t.length; e += 16) r = c, o = u, i = f, a = g, c = N(c, u, f, g, t[e], 7, -680876936), g = N(g, c, u, f, t[e + 1], 12, -389564586), f = N(f, g, c, u, t[e + 2], 17, 606105819), u = N(u, f, g, c, t[e + 3], 22, -1044525330), c = N(c, u, f, g, t[e + 4], 7, -176418897), g = N(g, c, u, f, t[e + 5], 12, 1200080426), f = N(f, g, c, u, t[e + 6], 17, -1473231341), u = N(u, f, g, c, t[e + 7], 22, -45705983), c = N(c, u, f, g, t[e + 8], 7, 1770035416), g = N(g, c, u, f, t[e + 9], 12, -1958414417), f = N(f, g, c, u, t[e + 10], 17, -42063), u = N(u, f, g, c, t[e + 11], 22, -1990404162), c = N(c, u, f, g, t[e + 12], 7, 1804603682), g = N(g, c, u, f, t[e + 13], 12, -40341101), f = N(f, g, c, u, t[e + 14], 17, -1502002290), u = N(u, f, g, c, t[e + 15], 22, 1236535329), c = z(c, u, f, g, t[e + 1], 5, -165796510), g = z(g, c, u, f, t[e + 6], 9, -1069501632), f = z(f, g, c, u, t[e + 11], 14, 643717713), u = z(u, f, g, c, t[e], 20, -373897302), c = z(c, u, f, g, t[e + 5], 5, -701558691), g = z(g, c, u, f, t[e + 10], 9, 38016083), f = z(f, g, c, u, t[e + 15], 14, -660478335), u = z(u, f, g, c, t[e + 4], 20, -405537848), c = z(c, u, f, g, t[e + 9], 5, 568446438), g = z(g, c, u, f, t[e + 14], 9, -1019803690), f = z(f, g, c, u, t[e + 3], 14, -187363961), u = z(u, f, g, c, t[e + 8], 20, 1163531501), c = z(c, u, f, g, t[e + 13], 5, -1444681467), g = z(g, c, u, f, t[e + 2], 9, -51403784), f = z(f, g, c, u, t[e + 7], 14, 1735328473), u = z(u, f, g, c, t[e + 12], 20, -1926607734), c = X(c, u, f, g, t[e + 5], 4, -378558), g = X(g, c, u, f, t[e + 8], 11, -2022574463), f = X(f, g, c, u, t[e + 11], 16, 1839030562), u = X(u, f, g, c, t[e + 14], 23, -35309556), c = X(c, u, f, g, t[e + 1], 4, -1530992060), g = X(g, c, u, f, t[e + 4], 11, 1272893353), f = X(f, g, c, u, t[e + 7], 16, -155497632), u = X(u, f, g, c, t[e + 10], 23, -1094730640), c = X(c, u, f, g, t[e + 13], 4, 681279174), g = X(g, c, u, f, t[e], 11, -358537222), f = X(f, g, c, u, t[e + 3], 16, -722521979), u = X(u, f, g, c, t[e + 6], 23, 76029189), c = X(c, u, f, g, t[e + 9], 4, -640364487), g = X(g, c, u, f, t[e + 12], 11, -421815835), f = X(f, g, c, u, t[e + 15], 16, 530742520), u = X(u, f, g, c, t[e + 2], 23, -995338651), c = F(c, u, f, g, t[e], 6, -198630844), g = F(g, c, u, f, t[e + 7], 10, 1126891415), f = F(f, g, c, u, t[e + 14], 15, -1416354905), u = F(u, f, g, c, t[e + 5], 21, -57434055), c = F(c, u, f, g, t[e + 12], 6, 1700485571), g = F(g, c, u, f, t[e + 3], 10, -1894986606), f = F(f, g, c, u, t[e + 10], 15, -1051523), u = F(u, f, g, c, t[e + 1], 21, -2054922799), c = F(c, u, f, g, t[e + 8], 6, 1873313359), g = F(g, c, u, f, t[e + 15], 10, -30611744), f = F(f, g, c, u, t[e + 6], 15, -1560198380), u = F(u, f, g, c, t[e + 13], 21, 1309151649), c = F(c, u, f, g, t[e + 4], 6, -145523070), g = F(g, c, u, f, t[e + 11], 10, -1120210379), f = F(f, g, c, u, t[e + 2], 15, 718787259), u = F(u, f, g, c, t[e + 9], 21, -343485551), c = P(c, r), u = P(u, o), f = P(f, i), g = P(g, a);
		return [c, u, f, g]
	}
	
	function N(t, n, e, r, o, i, a) {
		return R(n & e | ~n & r, t, n, o, i, a)
	}
	function R(t, n, e, r, o, i) {
		return P(Y(P(P(n, t), P(r, i)), o), e)
	}
	function P(t, n) {
		var e = (65535 & t) + (65535 & n);
		return (t >> 16) + (n >> 16) + (e >> 16) << 16 | 65535 & e
	}
	function Y(t, n) {
		return t << n | t >>> 32 - n
	}
	
	function z(t, n, e, r, o, i, a) {
		return R(n & r | e & ~r, t, n, o, i, a)
	}
	
	function X(t, n, e, r, o, i, a) {
		return R(n ^ e ^ r, t, n, o, i, a)
	}
	
	function F(t, n, e, r, o, i, a) {
		return R(e ^ (n | ~r), t, n, o, i, a)
	}
	function j(t) {
		var n = void 0,
			e = "";
		for (n = 0; n < 32 * t.length; n += 8) e += String.fromCharCode(t[n >> 5] >>> n % 32 & 255);
		return e
	}
	test = B(Q("` + param2 + `", "` + param1 + `"))
	`)
	value, _ := ctx.Get("test")
	strValue, _ := value.ToString()
	return strValue
}

func rn(param1 string, param2 string) string {
	ctx := otto.New()
	ctx.Run(`function rn(t, n) {
		for (var e = "", r = 0; r < t.length; r++) e += String.fromCharCode(n ^ t.charCodeAt(r));
		return e
	}
	test = rn("` + param1 + `", "` + param2 + `")`)
	value, _ := ctx.Get("test")
	strValue, _ := value.ToString()
	return strValue
}
