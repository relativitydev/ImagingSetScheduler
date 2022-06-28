﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;

namespace KCD_1041539.ImagingSetScheduler.Helper
{
    public class ObjectManagerHelper
    {
        private readonly IServicesProxyFactory _servicesProxyFactory;
        public static readonly Guid IMAGING_SET_SCHEDULER = new Guid("45C9FEB9-43D8-43FE-A216-85B1F062B0A7");
        public ObjectManagerHelper(IServicesProxyFactory servicesProxyFactory) 
        {
            _servicesProxyFactory = servicesProxyFactory;
        }

        public static async Task<List<RelativityObject>> RetrieveAllImagingSetSchedulesNotWaitingAsync(int workspaceId, IServicesProxyFactory servicesProxyFactory, IInstanceSettingManager instanceSettingManager)
        {
            int batchSize = await instanceSettingManager.GetIntegerValueAsync("Relativity.Imaging", "ImagingSetSchedulerBatchSize", 1000).ConfigureAwait(false);
            int iterator = 1;
            int totalCount;
            List<RelativityObject> res = new List<RelativityObject>();

            using (IObjectManager objectManager = servicesProxyFactory.CreateServiceProxy<IObjectManager>())
            {
                var queryRequest = new QueryRequest()
                {
                    ObjectType = new ObjectTypeRef { Guid = IMAGING_SET_SCHEDULER },
                    Fields = new List<FieldRef>
                    {
                        new FieldRef()
                        {
                            Guid = Constant.Guids.Field.ImagingSetScheduler.FREQUENCY
                        },
                        new FieldRef()
                        {
                            Guid = Constant.Guids.Field.ImagingSetScheduler.NAME
                        },
                        new FieldRef()
                        {
                            Guid = Constant.Guids.Field.ImagingSetScheduler.IMAGING_SET
                        },
                        new FieldRef()
                        {
                            Guid = Constant.Guids.Field.ImagingSetScheduler.TIME
                        },
                        new FieldRef()
                        {
                            Guid = Constant.Guids.Field.ImagingSetScheduler.LOCK_IMAGES_FOR_QC
                        },
                        new FieldRef()
                        {
                            Guid = Constant.Guids.Field.ImagingSetScheduler.LAST_RUN
                        },
                        new FieldRef()
                        {
                            Guid = Constant.Guids.Field.ImagingSetScheduler.NEXT_RUN
                        },
                        new FieldRef()
                        {
                            Name = "System Created By"
                        }
                    },
                    Condition = "(((NOT 'Status' ISSET) OR NOT ('Status' IN ['waiting'])))"
                };

                do
                {
                    QueryResult queryResult = await objectManager.QueryAsync(workspaceId, queryRequest, iterator, batchSize).ConfigureAwait(false);
                    totalCount = queryResult.TotalCount;
                    iterator += batchSize;
                    res.AddRange(queryResult.Objects);
                } while (iterator < totalCount);

                 
                return res;
            }
        }
    }
}