import sys;
import mari;

# def execFunc(command: mari.vgrainCommand, tools: mari.vgrainTools) -> any:
#     return command.get_content()

# app = mari.vgrain(sys.argv)

# app.run(execFunc);

def calc(command: mari.vgrainCommand, tools: mari.vgrainTools):
    """ Eval and calculate the mathematical result """
    formula = command.get_content()
    tools.get_logger.logInformation("Test log " + formula)
    tools.get_logger.logWarning("Test log warning" + str(type(formula)))
    tools.get_logger.logError("Test log error")
    
    for key in tools.get_config:
        tools.get_logger.logInformation("Cfg = " + key + ": " + str(tools.get_config[key]))
    
    result = "-------------" + str(formula) + "-------------"
    # tools.get_logger.logInformation("Result " + result)
    return result

app = mari.vgrain(sys.argv)

app.run(calc);
# app.test("5+69+9+azeazeaze", calc)