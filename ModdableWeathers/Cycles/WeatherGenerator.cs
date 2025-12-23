namespace ModdableWeathers.Cycles;

public class WeatherGenerator(
    WeatherHistoryRegistry historyRegistry,
    WeatherHistoryService historyService,
    ModdableWeatherRegistry weatherRegistry,
    WeatherCycleStageDefinitionService stateDefinition,
    ModdableWeatherModifierRegistry modifierRegistry
)
{

    public void EnsureWeatherGenerated(int cycle)
    {
        var target = cycle + 1;
        
        if (target <= historyRegistry.CycleCount) { return; }

        for (var cycleIndex = historyRegistry.CycleCount + 1; cycleIndex <= target; cycleIndex++)
        {
            var generatedCycle = GenerateWeatherCycle(cycleIndex);
            historyRegistry.AddWeatherCycles(generatedCycle, cycle);
        }

        historyService.UpdateReferences(cycle);
    }

    public WeatherCycle GenerateWeatherCycle(int cycleIndex)
    {
        StringBuilder log = new();
        log.AppendLine($"Generating weather for cycle {cycleIndex}:");

        WeatherCycleDecision decision = new()
        {
            Cycle = cycleIndex,
            Stages = [.. GetStages(cycleIndex, log, stateDefinition.StagesDefinitions)]
        };

        foreach (var stage in decision.Stages)
        {
            // First, the weather
            var w = stage.Weather = DecideWeather(stage, decision, log);

            // Then the length
            stage.Days = w.GetDuration(stage, decision, historyService);
            log.AppendLine($"; Duration: {stage.Days} days, stage effective {stage.GetDaysEffective()} days.");

            // Then, the modifiers
            stage.Modifiers.AddRange(DecideModifiers(stage, decision, log));
        }

        ModdableWeathersUtils.LogVerbose(log.ToString, "| ");

        return new(decision.Cycle, [..decision.Stages
            .Select(s => new WeatherCycleStage(
                s.StageIndex,
                s.IsBenign,
                s.Weather!.Id,
                [..s.Modifiers.Select(m => m.Id)],
                s.GetDaysEffective()
            ))
        ]);
    }

    IModdableWeather DecideWeather(WeatherCycleStageDecision stage, WeatherCycleDecision decision, StringBuilder log)
    {
        log.AppendLine($"Deciding weather for stage {stage.StageIndex} (Benign: {stage.IsBenign}):");

        IReadOnlyList<IModdableWeather> weathers = stage.IsBenign
            ? weatherRegistry.BenignWeathers
            : weatherRegistry.HazardousWeathers;

        var totalWeight = 0;
        List<(IModdableWeather weather, int weight)> weightedWeathers = [];

        foreach (var weather in weathers)
        {
            if (!weather.Enabled) { continue; }

            var chance = weather.GetChance(stage, decision, historyService);
            if (chance == 0) { continue; }

            totalWeight += chance;
            weightedWeathers.Add((weather, chance));
            log.AppendLine($"- {weather.Id} Chance: {chance}, select if roll from {totalWeight - chance} to {totalWeight - 1}");
        }

        if (totalWeight == 0 || weightedWeathers.Count == 0)
        {
            log.Append($"=> No valid weathers found. Using empty weather.");
            return stage.IsBenign ? weatherRegistry.EmptyBenignWeather : weatherRegistry.EmptyHazardousWeather;
        }

        var roll = Random.RandomRangeInt(0, totalWeight);
        var cumulative = 0;
        foreach (var (weather, weight) in weightedWeathers)
        {
            cumulative += weight;
            if (roll < cumulative)
            {
                log.Append($"=> Selected weather: {weather.Id} (Roll: {roll} < Cumulative: {cumulative})");
                return weather;
            }
        }

        throw new InvalidOperationException(); // Should not reach here
    }

    IEnumerable<IModdableWeatherModifier> DecideModifiers(WeatherCycleStageDecision stage, WeatherCycleDecision decision, StringBuilder log)
    {
        log.AppendLine($"Deciding modifiers for stage {stage.StageIndex}:");

        if (stage.Weather is null || stage.Weather.IsEmpty())
        {
            log.AppendLine("=> No weather assigned, skipping modifiers.");
            yield break;
        }

        foreach (var modifier in modifierRegistry.Modifiers)
        {
            log.Append($"- Modifier {modifier.Id}: ");
            var chance = modifier.GetChance(stage, decision, historyService);
            if (chance <= 0)
            {
                log.AppendLine($"Chance {chance}% => Not applied.");
                continue;
            }
            else if (chance >= 100)
            {
                log.AppendLine($"Chance {chance}% => Applied.");
                yield return modifier;
                continue;
            }
            else
            {
                var roll = Random.RandomRangeInt(0, 100);
                var applied = roll < chance;
                log.AppendLine($"Roll {roll} < Chance {chance} => {(applied ? "Applied" : "Not applied")}.");
                if (applied)
                {
                    if (modifier is IModdableWeatherDecisionModifier decisionModifier)
                    {
                        decisionModifier.ModifyDecision(stage, decision, historyService);
                    }

                    yield return modifier;
                }
            }
        }
    }

    IEnumerable<WeatherCycleStageDecision> GetStages(int cycleIndex, StringBuilder log, ImmutableArray<WeatherCycleStageDefinition> definitions)
    {
        var actualStageIndex = 0;

        log.AppendLine("Determining stages:");
        for (int i = 0; i < definitions.Length; i++)
        {
            log.Append($"- Stage {i}:");

            var def = definitions[i];
            if (def.SkipChance > 0)
            {
                if (def.SkipChance >= 100)
                {
                    log.AppendLine($" Skipped (100% chance).");
                    continue;
                }

                var roll = Random.RandomRangeInt(0, 100);
                var skip = roll < def.SkipChance;
                log.AppendLine($" Skip if {roll} < {def.SkipChance}: {(skip ? "Skipped" : "Not skipped")}.");

                if (skip) { continue; }
            }

            bool isBenign = false;
            log.Append($"  Weather Benign: ");
            if (def.BenignChance > 0)
            {
                if (def.BenignChance >= 100)
                {
                    isBenign = true;
                    log.AppendLine($" 100% chance => Benign.");
                }
                else
                {
                    var roll = Random.RandomRangeInt(0, 100);
                    isBenign = roll < def.BenignChance;
                    log.AppendLine($" Roll {roll}% < {def.BenignChance}% => {(isBenign ? "Benign" : "Hazardous")}.");
                }
            }
            else
            {
                log.AppendLine(" 0% chance => Hazardous.");
            }

            yield return new()
            {
                Cycle = cycleIndex,
                StageIndex = actualStageIndex++,
                IsBenign = isBenign,
                DaysMultiplier = def.LengthMultiplier,
            };
        }
    }

}
