import yfinance as yf
import pandas as pd
import json
from pathlib import Path
import os
currencies = [
    "EURUSD=X","JPY=X","GBPUSD=X","AUDUSD=X","NZDUSD=X",
   # "EURJPY=X","GBPJPY=X","EURGBP=X","EURCAD=X","EURSEK=X",
   # "EURCHF=X","EURHUF=X","CNY=X","HKD=X","SGD=X","INR=X",
    "MXN=X","PHP=X","IDR=X","THB=X","MYR=X","ZAR=X","RUB=X"
]

stocknames = [
    "AAPL", "MSFT", "NVDA", "AVGO", "CSCO", "ADBE", "CRM", "ORCL", 
   # "INTC", "QCOM", "AMD", "ADP", "CDNS", 
   # "SNPS", "KLAC", "LRCX", "AMAT", "V", "MA", "HPQ", "DELL", "FFIV",
   # "JPM", "BAC", "WFC", "GS", "MS", "AXP", "SPGI", "CME", 
   # "ICE", "BLK", "PNC", "XOM", "CVX", "FCX"
   # "JNJ", "UNH", "LLY", "PFE", "ABT", "DHR", "ISRG", 
   # "BIIB", "AMGN", "GILD", "MDT",  "CI",
   # "AMZN", "TSLA", "HD", "MCD", "NKE", "SBUX", "LOW", "BKNG", "MAR", 
   # "TJX", "PG", "KO", "CL", "KMB", "KR", "MDLZ",
    "GE", "HON", "BA", "NEE", "DUK", "SO", "EIX", "PLD",
]

etfnames = [
    "SPY", "QQQ", "DIA", "IVV", "VOO", "VTI", "IWM",
    #"XLK", "XLF", "XLE", "XLV", "XLY", "IWF", "IWD", "IJR",
    #"EFA", "VWO", "VEA", "EEM", "GLD", "FXI", "EWJ", "EWG",
    #"AGG", "BND", "LQD", "HYG", "IEF", "TLT", "MUB",
    #"VNQ", "IYR", "USO", "XLP", "GDX", "AMLP", "PFF", "DBC",
    "IBB", "SMH", "FDIS", "VHT", "IYF", "XRT", "CURE"
]

def prep(names): #make a long whitespace sparated string, that is the argument that Tickers() expects.
    tnameString = ""
    for name in names:
        tnameString = tnameString + " " + name

    tnameString.strip()
    return tnameString

def populate(ticker:yf.ticker.Ticker,tname,path_prefix):
    #here we need to:
    #1.save general info, description etc.
    #2.save prices data time series
    #3.save earnings maybe... idk
    fileName = tname + ".json"
    csvFileName = tname + ".csv"
    priceHistory = ticker.history(period="5y")
    tickInfo = ticker.get_info() #dictionary of the information about the etf
    summary = "N/A"
    keys = tickInfo.keys()
    if "longBusinessSummary" in keys:
        summary = tickInfo["longBusinessSummary"]
    else:
        summary = tickInfo["longName"]
    tinfo = {"description":summary,
            "name":tname,
            "longName":tickInfo["longName"]}

    infoJSON = json.dumps(tinfo)
    try:
        fl = open(os.path.join(path_prefix,fileName),"w") #write etf metadata to the json
        fl.write(infoJSON)
        fl.close()

        priceHistory.to_csv(os.path.join(path_prefix,csvFileName))

    except Exception as e:
        print(e)
    
def setupETFs(etfnames,path):
    tickerz = yf.Tickers(prep(etfnames)) #ticker list
    for t in etfnames:
        populate(tickerz.tickers[t],t,path)

def setupStocks(stocknames,path):
    tickerz = yf.Tickers(prep(stocknames)) #ticker list
    for t in stocknames:
        populate(tickerz.tickers[t],t,path)

def setupFX(forexnames,path):
    tickerz = yf.Tickers(prep(forexnames)) #ticker list
    for t in forexnames:
        populate(tickerz.tickers[t],t,path)

papa = Path(__file__).parent

etffolder = Path("etfs")
stocksfolder = Path("stocks")
fxfolder = Path("fxs")

etfpath = os.path.join(papa,etffolder)
stockpath = os.path.join(papa,stocksfolder)
fxpath = os.path.join(papa,fxfolder)

try:
    etffolder.mkdir()
    stocksfolder.mkdir()
    fxfolder.mkdir()
except Exception as e:
    print("ERROR BRUV")


    
    
#uncomment following lines to create the neccessary data, extract what needed dand make csv's. 
stocknames2 = ["AAPL"]
etfnames2 = ["SPY"]
currencies2 = ["EURUSD=X"]
setupETFs(etfnames,etfpath)
setupStocks(stocknames,stockpath)
setupFX(currencies,fxpath)