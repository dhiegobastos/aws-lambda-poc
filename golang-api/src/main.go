package main

import (
	"encoding/json"
	"math/rand"
	"net/http"
	"time"

	"github.com/aws/aws-lambda-go/events"
	"github.com/aws/aws-lambda-go/lambda"
)

var summaries = [10]string{"Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"}

type WeatherForecast struct {
	Date         time.Time
	TemperatureC int
	TemperatureF int
	Summary      string
}

func (w *WeatherForecast) weatherDiscovery() {
	s1 := rand.NewSource(time.Now().UnixNano())
	random := rand.New(s1)
	randomNumber := random.Intn(10)
	randomTemperature := random.Intn(50)

	w.Summary = summaries[randomNumber]
	w.Date = time.Now()
	w.TemperatureC = randomTemperature
	w.TemperatureF = 32 + int(float64(w.TemperatureC)/0.5556)
}

func handlerWeatherForecast(req events.APIGatewayProxyRequest) (events.APIGatewayProxyResponse, error) {
	weather := WeatherForecast{}
	weather.weatherDiscovery()

	weatherResponse, err := json.Marshal(weather)
	if err != nil {
		return events.APIGatewayProxyResponse{
			StatusCode: http.StatusInternalServerError,
			Body:       err.Error(),
		}, nil
	}

	resp := events.APIGatewayProxyResponse{Headers: make(map[string]string)}
	resp.Headers["Access-Control-Allow-Origin"] = "*"

	response, err := events.APIGatewayProxyResponse{
		StatusCode: http.StatusOK,
		Body:       string(weatherResponse),
		Headers:    make(map[string]string),
	}, nil

	response.Headers["Content-Type"] = "application/json"

	return response, err
}

func router(req events.APIGatewayProxyRequest) (events.APIGatewayProxyResponse, error) {

	if req.Path == "/weather" {
		if req.HTTPMethod == "GET" {
			return handlerWeatherForecast(req)
		}
	}

	return events.APIGatewayProxyResponse{
		StatusCode: http.StatusMethodNotAllowed,
		Body:       http.StatusText(http.StatusMethodNotAllowed),
	}, nil
}

func main() {
	lambda.Start(router)
}
