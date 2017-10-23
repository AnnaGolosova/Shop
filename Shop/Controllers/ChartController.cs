using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.DataVisualization.Charting;
using System.Drawing;
using System.IO;
using Shop.Models;
using Shop.Global;

namespace Shop.Controllers
{
    public class ChartController : BaseController
    {
         static StatisticParamsForSend  _params;

        StatisticParamsForSend Params {
            get
            {
                if (_params == null)
                    _params = new StatisticParamsForSend();
                return _params;
            }
            set
            {
                if (_params == null)
                    _params = new StatisticParamsForSend();
                if(value != null)
                {
                    try
                    {
                        _params.StartDate = value.StartDate;
                        _params.EndDate = value.EndDate;
                        _params.categoryId = value.categoryId;
                    }
                    catch(Exception ex)
                    {
                        //
                    }
                }
            }
        }

        public FileContentResult CreateChart()
        {
            var dates = repository.GetStatistic(Params);
            var chart = new Chart();
            chart.Width = 700;
            chart.Height = 300;
            chart.BackColor = Color.FromArgb(211, 223, 240);
            chart.BorderlineDashStyle = ChartDashStyle.Solid;
            chart.BackSecondaryColor = Color.White;
            chart.BackGradientStyle = GradientStyle.TopBottom;
            chart.BorderlineWidth = 1;
            chart.Palette = ChartColorPalette.BrightPastel;
            chart.BorderlineColor = Color.FromArgb(26, 59, 105);
            chart.RenderType = RenderType.BinaryStreaming;
            chart.BorderSkin.SkinStyle = BorderSkinStyle.Emboss;
            chart.AntiAliasing = AntiAliasingStyles.All;
            chart.TextAntiAliasingQuality = TextAntiAliasingQuality.Normal;
            chart.Titles.Add(new Title()
            {
                Text = "Result Chart",
                ShadowColor = Color.FromArgb(32, 0, 0, 0),
                Font = new Font("Trebuchet MS", 14F, FontStyle.Bold),
                ShadowOffset = 3,
                ForeColor = Color.FromArgb(26, 59, 105)
            }
                );
            chart.Legends.Add(new Legend()
            {
                Name = "Result Chart",
                Docking = Docking.Bottom,
                Alignment = StringAlignment.Center,
                BackColor = Color.Transparent,
                Font = new Font(new FontFamily("Trebuchet MS"), 9),
                LegendStyle = LegendStyle.Row
            });
            chart.Series.Add(CreateSeries(dates, SeriesChartType.Line, Color.Red));
            chart.ChartAreas.Add(new ChartArea()
            {

                Name = "Result Chart",
                BackColor = Color.Transparent,
            });
            chart.ChartAreas.First().AxisX.IsLabelAutoFit = false;
            chart.ChartAreas.First().AxisY.IsLabelAutoFit = false;
            chart.ChartAreas.First().AxisX.LabelStyle.Font = new Font("Verdana,Arial,Helvetica,sans-serif", 8F, FontStyle.Regular);
            chart.ChartAreas.First().AxisY.LabelStyle.Font = new Font("Verdana,Arial,Helvetica,sans-serif", 8F, FontStyle.Regular);
            chart.ChartAreas.First().AxisY.LineColor = Color.FromArgb(64, 64, 64, 64);
            chart.ChartAreas.First().AxisX.LineColor = Color.FromArgb(64, 64, 64, 64);
            chart.ChartAreas.First().AxisY.MajorGrid.LineColor = Color.FromArgb(64, 64, 64, 64);
            chart.ChartAreas.First().AxisX.MajorGrid.LineColor = Color.FromArgb(64, 64, 64, 64);
            chart.ChartAreas.First().AxisX.Interval = 1;

            var ms = new MemoryStream();
            chart.SaveImage(ms);
            return File(ms.GetBuffer(), @"image/png");
        }

        [NonAction]
        public Series CreateSeries(IList<Tuple<int, string>> results,
            SeriesChartType chartType,
            Color color)
        {
            var seriesDetail = new Series();
            seriesDetail.Name = "Result Chart";
            seriesDetail.IsValueShownAsLabel = false;
            seriesDetail.Color = color;
            seriesDetail.ChartType = chartType;
            seriesDetail.BorderWidth = 2;
            seriesDetail["DrawingStyle"] = "Cylinder";
            seriesDetail["PieDrawingStyle"] = "SoftEdge";
            DataPoint point;

            foreach (var result in results)
            {
                point = new DataPoint();
                point.AxisLabel = result.Item2;
                point.YValues = new double[] { result.Item1 };
                seriesDetail.Points.Add(point);
            }
            seriesDetail.ChartArea = "Result Chart";

            return seriesDetail;
        }

        [NonAction]
        public ChartArea CreateChartArea()
        {
            var chartArea = new ChartArea();
            chartArea.AxisX.MajorGrid.Enabled = false;
            chartArea.AxisY.MajorGrid.Enabled = false;
            return chartArea;
        }

        [Authorize(Roles = "admin")]
        public ActionResult OrderStatistic(StatisticParamsForSend Params = null, int ContentState = 0, int categoryId = 0)
        {
            Params.categoryId = categoryId;
            Params.ContentState = ContentState;
            this.Params = Params;

            ViewBag.Categories = repository.GetCategories();
            ViewBag.isStatisticView = true;
            return View(Params);
        }
    }
}