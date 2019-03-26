# Language Description

## Grammar
### Part 1: Common program structure
>// Arbitrary sequence of data declarations, modules, routines and code blocks
* **Program** : { Annotation | Data | Module | Routine | Code }
* **Annotation** : `pragma` PragmaDeclaration { `,` PragmaDeclaration } `;`
>// Named sequence of literal values
* **Data** : `data` Identifier [ Literal { `,` Literal } ] `end`
>// A named collection of resources: data and routines
* **Module** : `module` Identifier { VarDeclaration | Routine } `end`
* **Code** : `code` { VarDeclaration | Statement } `end`

---

* **PragmaDeclaration** : Identifier `(` [ Text ] `)`
* **VarDeclaration** : Variable | Constant
* **Variable** : Type VarDefinition { `,` VarDefinition } `;`
* **Type** : `int` | `short` | `byte`
* **VarDefinition** : Identifier [ `:=` Expression ] | Identifier `[` Expression `]`
* **Constant** : `const` ConstDefinition { `,` ConstDefinition } `;`
* **ConstDefinition** : Identifier `=` Expression

---

* **Routine** : [ Attribute ] `routine` Identifier [ Parameters ] [ Results ] ( `;` | RoutineBody )
* **Attribute** : `start` | `entry`
* **Parameters** : `(` Parameter { `,` Parameter } `)`
* **Parameter** : Type Identifier | Register
* **Results** : `:` Register { `,` Register }
* **RoutineBody** : `do` { VarDeclaration | Statement } `end`
* **Statement** : [ Label ] ( AssemblyBlock | ExtensionStatement )
* **Label** : `<` Identifier `>`

---

### Part XXX: directives, assembly statements and registers
* **AssemblyBlock**: `asm`  
&emsp;( AssemblyStatement `;`  
&emsp;| AssemblyStatement { `,` AssemblyStatement } `end` )
* **AssemblyStatement**  
&emsp;: `skip` // NOP  
&emsp;| `stop` // STOP  
&emsp;| `format` ( 8 | 16 | 32 ) // format of next command  
&emsp;| Register `:=` `*`Register // Rj := \*Ri LD    
&emsp;| `*`Register `:=` Register // \*Rj := Ri ST  
&emsp;| Register `:=` Register // Rj := Ri MOV  
&emsp;| Register `:=` Expression // Rj := Const LDC    
&emsp;| Register `+=` Register // Rj += Ri ADD  
&emsp;| Register `-=` Register // Rj -= Ri SUB  
&emsp;| Register `>>=` Register // Rj >>= Ri ASR  
&emsp;| Register `<<=` Register // Rj <<= Ri ASL  
&emsp;| Register `|=` Register // Rj |= Ri OR  
&emsp;| Register `&=` Register // Rj &= Ri AND  
&emsp;| Register `^=` Register // Rj ^= Ri XOR  
&emsp;| Register `<=` Register // Rj <= Ri LSL  
&emsp;| Register `>=` Register // Rj >= Ri LSR  
&emsp;| Register `?=` Register // Rj ?= Ri CND  
&emsp;| `if` Register `goto` Register // if Ri goto Rj CBR
* **Register** : R0 | R1 | ... | R30 | R31

---

* **ExtensionStatement** : Assignment | Swap | Call | If | Loop | Break | Goto
* **Loop** : For | While | LoopBody
* **For** : `for` Identifier [ `from` Expression ] [ `to` Expression] [ `step` Expression ] LoopBody
* **While** : `while` Expression LoopBody
* **LoopBody** : `loop` BlockBody `end`
* **BlockBody** : { Statement }
* **Break** : `break` `;`
* **Goto** : `goto` Identifier `;`
* **Assignment** : Primary `:=` Expression `;`
* **Swap** : Primary `<=>` Primary `;`

---

* **If** : `if` Expression `do` BlockBody ( `end` | `else` BlockBody `end` )
* **Call** : [ Identifier`.` ] Identifier CallArgs `;`
* **CallArgs** : `(` [ Expression { , Expression } ] `)`

---

* **Expression** : Operand [ Operator Operand ]
* **Operator** : `+` | `-` | `*` | `&` | `|` | `^` | `?` | CompOperator
* **CompOperator** : `=` | `/=` | `<` | `>`
* **Operand** : Receiver | Reference | Literal
* **Primary** : Receiver | Dereference | ExplicitAddress
* **Receiver** : Identifier | ArrayAccess | Register
* **ArrayAccess** : Identifier`[`Expression`]`
* **Reference** : `&` Identifier
* **Dereference** : `*` ( Identifier | Register )
* **ExplicitAddress** : `*` Literal

## Terminals

* **Identifier** : *(_a-zA-Z0-9)+*
* **Literal**: *(0-9)+*
* **Text**: *(\\,\\.\\-_a-zA-Z0-9)+*

## Default values

* Default attribute for function is `entry`
* In 'for' loop:
    1. Default `from` is current value is variable exists or 0
    2. Default `to` is infinity, thus the loop would stop only on 'break' command
    3. Default `step` is '1'
* Uninitialized variables can contain any garbage in it.

## Limitations

* Accessed functions and modules has to be already defined. For functions, at least it's interface has to be defined.  
* There is no functions\module overloading.
* `entry` functions must have function body, `start` functions are declarations of interface and can't have function body.
* Constants can't be changed, thus they can't be used in 'for' loops.
* Array size must be non-negative and compile-time constant.
* 'Expression' in assembly-statement assignment must be compile-time constant.
* 'Directive' affects only the next 'Assembler statement'.
* 'Code' block can be used only once.
* Language does not support floating point and unsigned values.

## Annotations

Allows to specify some compile-time flags.  List of **possible** flags
* `save(`Registers list`)` - tells compiler not to use this registers indirectly
* `prefer(`Registers list`)` - tells compiler to prioritize usage of given registers
* `optimizer(`Enable | Disable`)` - tells compiler to enable/disable some code optimization techniques

## Comments

Language supports only one-line comments that starts with `//`

## Future plans

[ ] Allow functions overloading  
[ ] Allow multiple breaks at once: requires grammar change  
[ ] Add operations on array: 'sizeof(\_)' and negative indexes usage  
[ ] Add array and struct default initializer: via '{\_, \_, ..}' construction assignment  
[ ] Add custom structs support: requires grammar change   
[ ] Add loops with post conditions (i.e. do .. while)
[ ] Add support for LDA assembly instructions

### FP: changes for structs support

* **Type** : `int` | `short` | `byte` | Identifier
* **Struct** : `struct` Identifier { VarDeclaration } `end`
