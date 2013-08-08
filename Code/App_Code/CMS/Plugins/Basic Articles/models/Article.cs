﻿using System;
using CMS.Base;
using UberLib.Connector;

namespace CMS.BasicArticles
{
    public class Article
    {
        private enum Fields
        {
            None = 0,
            ThreadUUID = 1,
            Title = 2,
            TextRaw = 4,
            TextCache = 8,
            DateTimeCreated = 16,
            DateTimeModified = 32,
            Published = 64,
            Comments = 128,
            HTML = 256,
            HidePanel = 512,
            UserIDAuthor = 1024,
            UserIDPublisher = 2048,
            All = 4095
        }
        /// <summary>
        /// The status of the persistence of the model.
        /// </summary>
        public enum PersistStatus
        {
            Error,
            Success
        }
        // Fields ******************************************************************************************************
        private bool        persisted;              // Indicates if the model has been persisted.
        private Fields      modified;               // Indicates modified fields.
        private UUID        uuidArticle,            // The UUID of this article.
                            uuidThread;             // The UUID of the article thread.
        private string      title,                  // The title of the article.
                            textRaw,                // The raw markup of the article.
                            textCache;              // The parsed and formatted markup, cached for speed.
        private DateTime    datetimeCreated,        // The date and time of when the article was created.
                            datetimeModified;       // The date and time of when the article was modified.
        private bool        published,              // Indicates if the article has been published.
                            comments,               // Indicates if the article should display comments.
                            html,                   // Indicates if the article should allow HTML.
                            hidePanel;              // Indicates if the article should display an options panel.
        private int         useridAuthor,           // The identifier of the user (BSA) which authored the article.
                            useridPublisher;        // The identifier of the user (BSA) which published the article.
        // Methods - Constructors **************************************************************************************
        public Article()
        {
            this.persisted = false;
            this.modified = Fields.None;
        }
        // Methods - Database Persistence ******************************************************************************
        /// <summary>
        /// Loads a persisted model from the database.
        /// </summary>
        /// <param name="conn">Database connector.</param>
        /// <param name="uuidArticle">Identifier of the article.</param>
        /// <returns>Model or null.</returns>
        public Article load(Connector conn, UUID uuidArticle)
        {
            PreparedStatement ps = new PreparedStatement("SELECT * FROM ba_view_load_article WHERE uuid_article_raw=?uuid;");
            ps["uuid"] = uuidArticle.Bytes;
            return load(conn, ps);
        }
        /// <summary>
        /// Loads a persisted model from the database; the raw text is not loaded.
        /// </summary>
        /// <param name="conn">Database connector.</param>
        /// <param name="uuidArticle">Identifier of the article.</param>
        /// <returns>Model or null.</returns>
        public Article loadRendered(Connector conn, UUID uuidArticle)
        {
            PreparedStatement ps = new PreparedStatement("SELECT * FROM ba_view_load_article_raw WHERE uuid_article_raw=?uuid;");
            ps["uuid"] = uuidArticle.Bytes;
            return load(conn, ps);
        }
        /// <summary>
        /// Loads a persisted model from the database; the rendered text is not loaded.
        /// </summary>
        /// <param name="conn">Database connector.</param>
        /// <param name="uuidArticle">Identifier of the article.</param>
        /// <returns>Model or null.</returns>
        public Article loadRaw(Connector conn, UUID uuidArticle)
        {
            PreparedStatement ps = new PreparedStatement("SELECT * FROM ba_view_load_article_rendered WHERE uuid_article_raw=?uuid;");
            ps["uuid"] = uuidArticle.Bytes;
            return load(conn, ps);
        }
        /// <summary>
        /// Loads a persisted model from the database.
        /// </summary>
        /// <param name="conn">Database connector.</param>
        /// <param name="ps">The prepared statement to be executed and read.</param>
        /// <returns>Model or null.</returns>
        public Article load(Connector conn, PreparedStatement ps)
        {
            Result r = conn.queryRead(ps);
            return r.Count == 1 ? load(r[0]) : null;
        }
        /// <summary>
        /// Loads a persisted model from the database.
        /// </summary>
        /// <param name="row">Database tuple/row.</param>
        /// <returns>Model or null.</returns>
        public Article load(ResultRow row)
        {
            Article a = new Article();
            a.uuidArticle = UUID.parse(row.get2<string>("uuid_article"));
            a.uuidThread = UUID.parse(row.get2<string>("uuid_thread"));
            a.title = row.get2<string>("title");
            a.textRaw = row.contains("text_raw") ? row.get2<string>("text_raw") : null;
            a.textCache = row.contains("text_cache") ? row.get2<string>("text_cache") : null;
            a.datetimeCreated = row.get2<DateTime>("datetime_created");
            a.datetimeModified = row.get2<DateTime>("datetime_modified");
            a.published = row.get2<string>("published").Equals("1");
            a.comments = row.get2<string>("comments").Equals("1");
            a.html = row.get2<string>("html").Equals("1");
            a.hidePanel = row.get2<string>("hide_panel").Equals("1");
            a.useridAuthor = row.get2<int>("userid_author");
            a.useridPublisher = row.get2<int>("userid_publisher");
            return a;
        }
        /// <summary>
        /// Persists the model.
        /// </summary>
        /// <param name="conn">Database connector.</param>
        /// <returns>The status of persisting the model.</returns>
        public PersistStatus save(Connector conn)
        {
            // Check some fields have been modified
            if (modified == Fields.None)
                return PersistStatus.Error;
            // Validate model data

            // Compile SQL
            SQLCompiler sql = new SQLCompiler();
            if ((modified & Fields.ThreadUUID) == Fields.ThreadUUID)
                sql["uuid_thread"] = uuidThread.Bytes;
            if ((modified & Fields.Title) == Fields.Title)
                sql["title"] = title;
            if ((modified & Fields.TextRaw) == Fields.TextRaw)
                sql["text_raw"] = textRaw;
            if ((modified & Fields.TextCache) == Fields.TextCache)
                sql["text_cache"] = textCache;
            if ((modified & Fields.DateTimeCreated) == Fields.DateTimeCreated)
                sql["datetime_created"] = datetimeCreated;
            if ((modified & Fields.DateTimeModified) == Fields.DateTimeModified)
                sql["datetime_modified"] = datetimeModified;
            if ((modified & Fields.Published) == Fields.Published)
                sql["published"] = published ? "1" : "0";
            if ((modified & Fields.Comments) == Fields.Comments)
                sql["comments"] = comments ? "1" : "0";
            if ((modified & Fields.HTML) == Fields.HTML)
                sql["html"] = html ? "1" : "0";
            if ((modified & Fields.HidePanel) == Fields.HidePanel)
                sql["hide_panel"] = hidePanel ? "1" : "0";
            if ((modified & Fields.UserIDAuthor) == Fields.UserIDAuthor)
                sql["userid_author"] = useridAuthor;
            if ((modified & Fields.UserIDPublisher) == Fields.UserIDPublisher)
                sql["userid_publisher"] = useridPublisher;
            // Execute SQL
            if (persisted)
            {
                sql.UpdateAttribute = "uuid_article";
                sql.UpdateValue = uuidArticle.Bytes;
                sql.executeUpdate(conn, "ba_article");
            }
            else
            {
                sql["uuid_thread"] = uuidThread.Bytes;
                sql.executeInsert(conn, "ba_article");
            }
            return PersistStatus.Success;
        }
        // Methods - Properties ****************************************************************************************
        /// <summary>
        /// The UUID of this article.
        /// </summary>
        public UUID UUIDArticle
        {
            get
            {
                return uuidArticle;
            }
        }
        /// <summary>
        /// The UUID of the thread this article belongs to.
        /// </summary>
        public UUID UUIDThread
        {
            get
            {
                return uuidThread;
            }
        }
        /// <summary>
        /// The title of the article.
        /// </summary>
        public string Title
        {
            get
            {
                return title;
            }
            set
            {
                title = value;
                modified |= Fields.Title;
            }
        }
        /// <summary>
        /// The raw markup of the article.
        /// </summary>
        public string TextRaw
        {
            get
            {
                return textRaw;
            }
            set
            {
                textRaw = value;
                modified |= Fields.TextRaw;
            }
        }
        /// <summary>
        /// The parsed and rendered HTML of the article.
        /// </summary>
        public string TextCache
        {
            get
            {
                return textCache;
            }
            set
            {
                textCache = value;
                modified |= Fields.TextCache;
            }
        }
        /// <summary>
        /// The date and time of when the article was created.
        /// </summary>
        public DateTime DateTimeCreated
        {
            get
            {
                return datetimeCreated;
            }
            set
            {
                datetimeCreated = value;
                modified |= Fields.DateTimeCreated;
            }
        }
        /// <summary>
        /// The date and time of when the article was modified.
        /// </summary>
        public DateTime DateTimeModified
        {
            get
            {
                return datetimeModified;
            }
            set
            {
                datetimeModified = value;
                modified |= Fields.DateTimeModified;
            }
        }
        /// <summary>
        /// Indicates if the article has been published.
        /// </summary>
        public bool Published
        {
            get
            {
                return published;
            }
            set
            {
                published = value;
                modified |= Fields.Published;
            }
        }
        /// <summary>
        /// Indicates if to display comments.
        /// </summary>
        public bool Comments
        {
            get
            {
                return comments;
            }
            set
            {
                comments = value;
                modified |= Fields.Comments;
            }
        }
        /// <summary>
        /// Indicates if the article should allow HTML, else it will be escaped when rendered.
        /// </summary>
        public bool HTML
        {
            get
            {
                return html;
            }
            set
            {
                html = value;
                modified |= Fields.HTML;
            }
        }
        /// <summary>
        /// Indicates if to hide the edit panel.
        /// </summary>
        public bool HidePanel
        {
            get
            {
                return hidePanel;
            }
            set
            {
                hidePanel = value;
                modified |= Fields.HidePanel;
            }
        }
        /// <summary>
        /// The user identifier (BSA) of the author.
        /// </summary>
        public int UserIdAuthor
        {
            get
            {
                return useridAuthor;
            }
            set
            {
                useridAuthor = value;
                modified |= Fields.UserIDAuthor;
            }
        }
        /// <summary>
        /// The user identifier (BSA) of the publisher.
        /// </summary>
        public int UserIdPublisher
        {
            get
            {
                return useridPublisher;
            }
            set
            {
                useridPublisher = value;
                modified |= Fields.UserIDPublisher;
            }
        }
    }
}