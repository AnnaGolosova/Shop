using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Shop.Models
{
    public class StatisticParamsForSend
    {
        DateTime _startDate;
        DateTime _endDate;
        public int categoryId;
        public int ContentState;

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode = true)]
        public DateTime StartDate
        {
            get
            {
                return _startDate;
            }
            set
            {
                _startDate = value;
            }
        }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode = true)]
        public DateTime EndDate
        {
            get
            {
                return _endDate;
            }
            set
            {
                if (value > DateTime.Now)
                    _endDate = DateTime.Now;
                if(value >= _startDate)
                    _endDate = value;
                else
                {
                    _endDate = _startDate.AddDays(1);
                    throw new Exception("Неверное время окончания");
                }
            }
        }

        public StatisticParamsForSend()
        {
            StartDate = DateTime.Now.AddMonths(-1);
            EndDate = DateTime.Now;
            categoryId = 0;
            ContentState = 0;
        }
    }
}