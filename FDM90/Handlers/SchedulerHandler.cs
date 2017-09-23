using FDM90.Models;
using FDM90.Models.Helpers;
using FDM90.Repository;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace FDM90.Handlers
{
    public class SchedulerHandler : ISchedulerHandler
    {
        private IRepository<ScheduledPost> _schedulerRepo;
        private IReadMultipleSpecific<ScheduledPost> _schedulerMultiReadRepo;
        private IFacebookHandler _facebookHandler;
        private ITwitterHandler _twitterHandler;
        private IUserHandler _userHandler;
        private List<IMediaHandler> _mediaHandlers = new List<IMediaHandler>();
        private IFileHelper _fileHelper;

        public SchedulerHandler(IRepository<ScheduledPost> schedulerRepo, IFacebookHandler facebookHandler, ITwitterHandler twitterHandler, IUserHandler userHandler, IFileHelper fileHelper)
        {
            _schedulerRepo = schedulerRepo;
            _schedulerMultiReadRepo = (IReadMultipleSpecific<ScheduledPost>)schedulerRepo;
            _facebookHandler = facebookHandler;
            _twitterHandler = twitterHandler;
            _userHandler = userHandler;
            _mediaHandlers.AddRange(new IMediaHandler[] { _facebookHandler, twitterHandler });
            _fileHelper = fileHelper;
        }

        public SchedulerHandler():this(new SchedulerRepository(), new FacebookHandler(), new TwitterHandler(), new UserHandler(), new FileHelper())
        {

        }

        public void CreateScheduledPost(ScheduledPost newPost)
        {
            newPost.PostId = Guid.NewGuid();
            _schedulerRepo.Create(newPost);
        }

        public void UpdateScheduledPost(ScheduledPost updatedPost)
        {
            _schedulerRepo.Update(updatedPost);
        }

        public IEnumerable<ScheduledPost> GetSchedulerPostsForUser(Guid userId)
        {
            return _schedulerMultiReadRepo.ReadMultipleSpecific(userId.ToString());
        }

        public void SchedulerPostsForTime(DateTime currentTime)
        {
            foreach(ScheduledPost post in _schedulerMultiReadRepo.ReadMultipleSpecific(currentTime.ToString()))
            {
                PostNow(post);
                DeleteScheduledPost(post);
            }
        }

        public void DeleteScheduledPost(ScheduledPost postToDelete)
        {
            _schedulerRepo.Delete(new ScheduledPost() { PostId = postToDelete.PostId });
            if (!string.IsNullOrWhiteSpace(postToDelete.AttachmentPath))
            {
                _fileHelper.DeleteFile(postToDelete.AttachmentPath.Replace('~', '\\'));
            }
        }

        public void DeletePostImage(Guid postId, string imagePath)
        {
            _fileHelper.DeleteFile(imagePath.Replace('~', '\\'));
        }

        public string CheckPostText(string textToPost, string medias, Guid userId)
        {
            string errorMessage = string.Empty;

            errorMessage = PostEthicalHelper.CheckTextForIssues(textToPost);

            User postingUser = _userHandler.GetUser(userId.ToString());
            foreach (IMediaHandler mediaHandler in _mediaHandlers.Where(w =>
                                            postingUser.GetIntegratedMediaChannels().Contains(w.MediaName)
                                                    && medias.Split(',').Contains(w.MediaName)))
            {
                if (textToPost.Count() > mediaHandler.MessageCharacterLimit)
                {
                    errorMessage += string.Format("Max characters exceeded for {0} ({1})", mediaHandler.MediaName, mediaHandler.MessageCharacterLimit);
                }

            }

            return errorMessage;
        }

        public void PostNow(ScheduledPost newPost)
        {
            User postingUser = _userHandler.GetUser(newPost.UserId.ToString());
            Dictionary<string, string> postParameters = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(newPost.PostText))
            {
                postParameters.Add("message", newPost.PostText);
            }

            if (!string.IsNullOrEmpty(newPost.AttachmentPath))
            {
                postParameters.Add("picture", newPost.AttachmentPath);
            }

            foreach (IMediaHandler mediaHandler in _mediaHandlers.Where(w =>
                                                        postingUser.GetIntegratedMediaChannels().Contains(w.MediaName)
                                                                && newPost.MediaChannels.Split(',').Contains(w.MediaName)))
            {
                mediaHandler.PostData(postParameters, newPost.UserId);
            }

            if (postParameters.ContainsKey("picture"))
            {
                _fileHelper.DeleteFile(postParameters["picture"].Replace('~', '\\'));
            }
        }

        public void DeleteScheduledPostForUser(Guid userId)
        {
            foreach(ScheduledPost post in GetSchedulerPostsForUser(userId))
            {
                DeleteScheduledPost(post);
            }
        }
    }
}