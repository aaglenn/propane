﻿module IR

open CGraph
open Common.Error

type CounterExample = 
    | UnusedPreferences of Map<int, Regex.T>
    | NoPathForRouters of Set<string>
    | InconsistentPrefs of CgState * CgState
    | UncontrollableEnter of string
    | UncontrollablePeerPreference of string

type Match = 
    | Peer of string 
    | State of string * string
    | PathRE of Regex.T
    | NoMatch

type Action = 
    | NoAction
    | SetComm of string
    | SetMed of int
    | PrependPath of int

type LocalPref = int
type Peer = string
type Import = Match * LocalPref
type Export = Peer * Action list

/// Result from compiling a single prefix

type DeviceConfig =
    {Originates: bool;
     Filters: ((Import * Export list) option) list}

type PredConfig = Predicate.T * Map<string, DeviceConfig>

type PrefixResult =
    {K: int option;
      BuildTime: int64;
      MinimizeTime: int64;
      OrderingTime: int64;
      ConfigTime: int64;
      CompressTime: int64;
      Config: PredConfig}

type CompileResult = Result<PrefixResult, CounterExample>

/// Result from compiling the entire policy

type DeviceAggregates = (Prefix.T list * seq<string>) list
type DeviceTags = ((string * Prefix.T list) * seq<string>) list
type DeviceMaxRoutes = (uint32 * seq<string>) list

type Aggregates = Map<string, (Prefix.T list * seq<string>) list>
type Tags = Map<string, ((string * Prefix.T list) * seq<string>) list>
type MaxRoutes = Map<string, (uint32 * seq<string>) list>

type DeviceControl = 
    {Aggregates: DeviceAggregates;
     Tags: DeviceTags;
     MaxRoutes: DeviceMaxRoutes}

type RouterConfig = 
    {Actions: (Predicate.T * DeviceConfig) list;
     Control: DeviceControl}

type T = Map<string, RouterConfig>

/// Convert per-prefix representation to a per-router representatoin
val joinConfigs: Aggregates * Tags * MaxRoutes -> PrefixResult list -> T

/// Debug config output
val format: T -> string

/// Generate the BGP match/action rules that are guaranteed to 
/// implement the user policy under all possible failure scenarios for a given prefix. 
/// This function returns either an intermediate representation (IR) 
/// for BGP policies, or a counterexample indicating why compilation will not work.
val compileToIR: string -> int -> Predicate.T -> Map<string, DeviceAggregates> -> Regex.REBuilder -> Regex.T list -> CompileResult

/// Compile to an intermediate representation for a given prefix. 
/// Gives a counterexample and quits the program if compilation is not possible.
val compileForSinglePrefix: string -> int -> Map<string, DeviceAggregates> -> Ast.PolicyPair -> PrefixResult

type Stats = 
    {TotalTime: int64;
     NumPrefixes: int;
     PrefixTime: int64;
     PerPrefixTimes: int64 array;
     PerPrefixBuildTimes: int64 array;
     PerPrefixMinTimes: int64 array;
     PerPrefixOrderTimes: int64 array;
     PerPrefixGenTimes: int64 array;
     PerPrefixCompressTimes: int64 array;
     JoinTime: int64;}

/// Compile for all prefixes
val compileAllPrefixes: string -> Topology.T -> Ast.PolicyPair list -> Ast.CConstraint list -> T * int option * Stats
