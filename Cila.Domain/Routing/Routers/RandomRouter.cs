﻿using System;
using Cila;
using Nethereum.Web3;

namespace Cila.Omnichain.Routers
{
	public class RandomRouter: IOmnichainRouter
	{
        private readonly ChainsService chainsService;

        public RandomRouter(ChainsService chainsService)
		{
            this.chainsService = chainsService;
        }

        public OmnichainRoute CalculateRoute(Command operation)
        {
            var chains = chainsService.GetAll();
			Random random = new Random();
       		int randomNumber = random.Next(chains.Count);
        	var randomElement = chains.ElementAt(randomNumber);
			return new OmnichainRoute{
				ChainId = randomElement.Id
			};
        }
    }
}