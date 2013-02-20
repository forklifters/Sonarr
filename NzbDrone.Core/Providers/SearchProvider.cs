﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Model;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Providers.Search;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Search;

namespace NzbDrone.Core.Providers
{
    public class SearchProvider
    {
        private readonly ISeriesService _seriesService;
        private readonly EpisodeService _episodeService;
        private readonly PartialSeasonSearch _partialSeasonSearch;
        private readonly ISeriesRepository _seriesRepository;

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public SearchProvider(ISeriesService seriesService, EpisodeService episodeService,
                              PartialSeasonSearch partialSeasonSearch,ISeriesRepository seriesRepository)
        {
            _seriesService = seriesService;
            _episodeService = episodeService;
            _partialSeasonSearch = partialSeasonSearch;
            _seriesRepository = seriesRepository;
        }

        public SearchProvider()
        {
        }

        public virtual List<int> SeasonSearch(ProgressNotification notification, int seriesId, int seasonNumber)
        {
            var series = _seriesRepository.Get(seriesId);

            if (series == null)
            {
                logger.Error("Unable to find an series {0} in database", seriesId);
                return new List<int>();
            }

            if (series.SeriesType == SeriesType.Daily)
            {
                logger.Trace("Daily series detected, skipping season search: {0}", series.Title);
                return new List<int>();
            }

            logger.Debug("Getting episodes from database for series: {0} and season: {1}", seriesId, seasonNumber);
            var episodes = _episodeService.GetEpisodesBySeason(seriesId, seasonNumber);

            if (episodes == null || episodes.Count == 0)
            {
                logger.Warn("No episodes in database found for series: {0} and season: {1}.", seriesId, seasonNumber);
                return new List<int>();
            }

            //Todo: Support full season searching
            return new List<int>();
        }

        public virtual List<int> PartialSeasonSearch(ProgressNotification notification, int seriesId, int seasonNumber)
        {
            var series = _seriesRepository.Get(seriesId);

            if (series == null)
            {
                logger.Error("Unable to find an series {0} in database", seriesId);
                return new List<int>();
            }

            if (series.SeriesType == SeriesType.Daily)
            {
                logger.Trace("Daily series detected, skipping season search: {0}", series.Title);
                return new List<int>();
            }

            var episodes = _episodeService.GetEpisodesBySeason(seriesId, seasonNumber);

            if (episodes == null || episodes.Count == 0)
            {
                logger.Warn("No episodes in database found for series: {0} Season: {1}.", seriesId, seasonNumber);
                return new List<int>();
            }

            return _partialSeasonSearch.Search(series, new {SeasonNumber = seasonNumber, Episodes = episodes}, notification);
        }
    }
}
