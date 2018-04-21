﻿//
//  @(#) WnsModel.cs
//
//  Project:    usis Push Notification Router
//  System:     Microsoft Visual Studio 2017
//  Author:     Udo Schäfer
//
//  Copyright (c) 2010-2017 usis GmbH. All rights reserved.

using PushSharp.Windows;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using usis.Framework;

namespace usis.PushNotification
{
    //  --------------
    //  WnsModel class
    //  --------------

    internal class WnsModel : IModel
    {
        #region IModel implementation

        #region channel

        #region ListChannelInfos method

        //  -----------------------
        //  ListChannelInfos method
        //  -----------------------

        IEnumerable<IChannelInfo> IModel.ListChannels(DBContext db)
        {
            foreach (var channel in db.WnsChannels.Include(nameof(Channel)))
            {
                if (channel.Deleted != 0) continue;

                var channelInfo = new WnsChannelInfo()
                {
                    PackageName = channel.PackageName,
                    Key = new WnsChannelKey(channel.PackageSid),
                    Description = channel.Channel.Description,
                    ClientSecret = channel.ClientSecret,
                    Created = DateTime.SpecifyKind(channel.Created, DateTimeKind.Utc),
                    Changed = channel.Changed.Later(channel.Channel.Changed)
                };
                if (channelInfo.Changed.HasValue) channelInfo.Changed = DateTime.SpecifyKind(channelInfo.Changed.Value, DateTimeKind.Utc);
                yield return channelInfo;
            }
        }

        #endregion ListChannelInfos method

        #region GetChannel method

        //  -----------------
        //  GetChannel method
        //  -----------------

        IChannel IModel.GetChannel(DBContext db, Guid channelId)
        {
            if (db == null) throw new ArgumentNullException(nameof(db));

            var query = from c in db.WnsChannels
                        where c.ChannelId == channelId
                        select c;
            return query.FirstOrDefault();
        }

        #endregion GetChannel method

        #region UpdateChannel method

        //  --------------------
        //  UpdateChannel method
        //  --------------------

        void IModel.UpdateChannel(DBContext db, IChannel channel, IChannelInfo channelInfo)
        {
            UpdateChannel(channel as WnsChannel, channelInfo as WnsChannelInfo);
        }

        private static void UpdateChannel(WnsChannel channel, WnsChannelInfo channelInfo)
        {
            if (channel == null) throw new ArgumentNullException(nameof(channel));
            if (channelInfo == null) throw new ArgumentNullException(nameof(channelInfo));
            channel.Channel.Description = channelInfo.Description;
            channel.PackageName = channelInfo.PackageName;
            channel.ClientSecret = channelInfo.ClientSecret;
        }

        #endregion UpdateChannel method

        #region FindChannel method

        //  ------------------
        //  FindChannel method
        //  ------------------

        IChannel IModel.FindChannel(DBContext db, ChannelKey channelKey)
        {
            return FindChannel(db, channelKey as WnsChannelKey, false);
        }

        #endregion FindChannel method

        #region FindOrCreateChannel method

        //  --------------------------
        //  FindOrCreateChannel method
        //  --------------------------

        IChannel IModel.FindOrCreateChannel(DBContext db, ChannelKey channelKey)
        {
            if (db == null) throw new ArgumentNullException(nameof(db));
            if (channelKey == null) throw new ArgumentNullException(nameof(channelKey));

            var wnsChannelKey = channelKey as WnsChannelKey;
            var channel = FindChannel(db, wnsChannelKey, true);
            if (channel == null)
            {
                channel = WnsChannel.NewChannel(wnsChannelKey);
                db.WnsChannels.Add(channel);
                return db.SaveChanges() > 0 ? channel : null;
            }
            else if (channel.Deleted != 0)
            {
                db.Entry(channel).Reference(c => c.Channel).Load();
                channel.Channel.ChannelType = channelKey.ChannelType;
                channel.Channel.Description = null;
                channel.Deleted = 0;
                channel.Channel.Deleted = 0;
                return db.SaveChanges() > 0 ? channel : null;
            }
            else
            {
                db.Entry(channel).Reference(c => c.Channel).Load();
                return channel;
            }
        }

        #endregion FindOrCreateChannel method

        #endregion channel

        #region receiver

        #region CreateReceiver method

        //  ---------------------
        //  CreateReceiver method
        //  ---------------------

        IReceiver IModel.CreateReceiver(DBContext db, IChannel channel, ReceiverKey receiverKey)
        {
            if (db == null) throw new ArgumentNullException(nameof(db));
            if (channel == null) throw new ArgumentNullException(nameof(channel));

            var wnsReceiverKey = receiverKey as WnsReceiverKey;
            Debug.Assert(wnsReceiverKey != null);
            var receiver = WnsReceiver.NewReceiver(channel.Channel, wnsReceiverKey.DeviceIdentifier);
            receiver.ChannelUri = wnsReceiverKey.ChannelUri.ToString();
            db.WnsReceivers.Add(receiver);
            return receiver;
        }

        #endregion CreateReceiver method

        #region ListReceivers method

        //  --------------------
        //  ListReceivers method
        //  --------------------

        IEnumerable<IReceiverInfo> IModel.ListReceivers(DBContext db, IChannel channel, DateTime? firstRegistration, bool includeExpired)
        {
            var query = from r in db.WnsReceivers
                        where r.Receiver.ChannelId == channel.Channel.ChannelId && r.Deleted == 0 && r.Receiver.Deleted == 0
                        select r;
            if (firstRegistration.HasValue) query = query.Where(e => e.Created >= firstRegistration);
            foreach (var device in query.Include(d => d.Receiver))
            {
                yield return new WnsReceiverInfo()
                {
                    DeviceIdentifier = device.DeviceIdentifier,
                    ChannelUri = device.ChannelUri,
                    ReceiverId = device.ReceiverId,
                    Name = device.Receiver.Name,
                    Account = device.Receiver.Account,
                    Groups = device.Receiver.Groups,
                    Info = device.Receiver.Info,
                    FirstRegistration = DateTime.SpecifyKind(device.Created, DateTimeKind.Utc),
                    LastRegistration = DateTime.SpecifyKind(device.Changed ?? device.Created, DateTimeKind.Utc),
                };
            }
        }

        #endregion ListReceivers method

        #endregion receiver

        #region FindReceiver method

        //  -------------------
        //  FindReceiver method
        //  -------------------

        IReceiver IModel.FindReceiver(DBContext db, ReceiverKey receiverKey)
        {
            if (db == null) throw new ArgumentNullException(nameof(db));

            var wnsReceiverKey = receiverKey as WnsReceiverKey;
            Debug.Assert(wnsReceiverKey != null);
            return db.WnsReceivers.Find(wnsReceiverKey.DeviceIdentifier);
        }

        #endregion FindReceiver method

        #region GetReceiver method

        //  ------------------
        //  GetReceiver method
        //  ------------------

        IReceiver IModel.GetReceiver(DBContext db, Guid receiverId)
        {
            if (db == null) throw new ArgumentNullException(nameof(db));

            var query = from r in db.WnsReceivers
                        where r.ReceiverId == receiverId
                        select r;
            return query.FirstOrDefault();
        }

        #endregion GetReceiver method

        #region UpdateReceiver method

        //  ---------------------
        //  UpdateReceiver method
        //  ---------------------

        void IModel.UpdateReceiver(IReceiver receiver, ReceiverKey receiverKey)
        {
            (receiver as WnsReceiver).ChannelUri = (receiverKey as WnsReceiverKey).ChannelUri.ToString();
        }

        #endregion UpdateReceiver method

        #region CreatePusher method

        //  -------------------
        //  CreatePusher method
        //  -------------------

        IPusher IModel.CreatePusher(IChannel channel, int keepChannelOpenInterval)
        {
            return new WnsPusher(channel as WnsChannel);
        }

        #endregion CreatePusher method

        #endregion IModel implementation

        #region private methods

        //  ------------------
        //  FindChannel method
        //  ------------------

        private static WnsChannel FindChannel(DBContext db, WnsChannelKey channelKey, bool includeDeleted)
        {
            var channel = db.WnsChannels.Find(channelKey.PackageSid);
            if (channel != null)
            {
                if (channel.Deleted != 0 && !includeDeleted) channel = null;
            }
            return channel;
        }

        //  -------------------
        //  FindReceiver method
        //  -------------------

        private static WnsReceiver FindReceiver(DBContext db, string deviceIdentifier)
        {
            var query = from r in db.WnsReceivers
                        where r.DeviceIdentifier == deviceIdentifier
                        select r;
            return query.FirstOrDefault();
        }

        #endregion private methods

        #region static methods

        #region SaveNotification method

        //  -----------------------
        //  SaveNotification method
        //  -----------------------

        internal static Guid SaveNotification(OperationResult result, Model model, string deviceIdentifier, string payload)
        {
            return model.UsingContext(db =>
            {
                var receiver = FindReceiver(db, deviceIdentifier);
                if (receiver != null)
                {
                    return Model.SaveNotification(result, db, receiver.ReceiverId, payload);
                }
                throw new RouterException(string.Format(CultureInfo.CurrentCulture, WnsStrings.ReceiverNotRegistered, deviceIdentifier));
            });
        }

        #endregion SaveNotification method

        #region CreateConfiguration method

        //  --------------------------
        //  CreateConfiguration method
        //  --------------------------

        internal static WnsConfiguration CreateConfiguration(WnsChannel channel)
        {
            return new WnsConfiguration(channel.PackageName, channel.PackageSid, channel.ClientSecret);
        }

        #endregion CreateConfiguration method

        #endregion static methods
    }
}

// eof "WnsModel.cs"