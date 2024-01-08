package main

import (
	"github.com/kataras/iris/v12"
	"log"
	"os"
	"pxgolang/pxgen"
)

type SolvePayload struct {
	ApiKey   string          `json:"apiKey"`
	ProxyURL string          `json:"proxyUrl"`
	Data     *pxgen.SiteData `json:"data"`
}

func SolvePerimeterXCookies(ctx iris.Context) {
	var payload SolvePayload

	if err := ctx.ReadJSON(&payload); err != nil {
		ctx.StatusCode(iris.StatusBadRequest)
		ctx.StopExecution()
		return
	}

	data := pxgen.PXDataStruct{}
	res, err := pxgen.SolveCookie(payload.ProxyURL, data, payload.Data)
	if err != nil {
		ctx.StatusCode(iris.StatusBadRequest)
		ctx.StopExecution()
		return
	}

	_, err = ctx.WriteString(res.PX3)
	if err != nil {
		ctx.StatusCode(iris.StatusBadRequest)
		ctx.StopExecution()
		return
	}
}

func SolvePerimeterXCaptcha(ctx iris.Context) {
	var payload SolvePayload

	if err := ctx.ReadJSON(&payload); err != nil {
		ctx.StatusCode(iris.StatusBadRequest)
		ctx.StopExecution()
		return
	}

	data := pxgen.PXDataStruct{}
	res, err := pxgen.SolveCookieCaptcha(payload.ProxyURL, payload.Data, payload.ApiKey, data)
	if err != nil {
		ctx.StatusCode(iris.StatusBadRequest)
		ctx.StopExecution()
		return
	}

	_, err = ctx.WriteString(res.PX3)
	if err != nil {
		ctx.StatusCode(iris.StatusBadRequest)
		ctx.StopExecution()
		return
	}
}

func main() {
	pxgen.Initialize() //Only need to run this once
	app := iris.New()
	app.Post("/solve/cookie", SolvePerimeterXCookies)
	app.Post("/solve/captcha", SolvePerimeterXCaptcha)

	listenPort := os.Getenv("SERVER_PORT")
	if len(listenPort) == 0 {
		listenPort = "8080"
	}

	err := app.Listen(":"+listenPort, iris.WithOptimizations)
	if err != nil {
		log.Fatal(err.Error())
	}

	/*
	 * Use SolveCookie method with your proxy, and set px3 cookie equal to res.PX3 in either header or cookie jar
	 * If your request passes through you're good to go,
	 * If you get PX block again, try solving cookie normally 2x more.
	 * If you still get a captcha run pxgen.SolveCookieCaptcha with same params as SolveCookie, this will solve a captcha on the proxy and return a cookie that you will need to set
	 */
}
