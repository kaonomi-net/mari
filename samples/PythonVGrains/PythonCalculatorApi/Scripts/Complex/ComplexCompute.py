import sys
import os
import mari

def compute(command: mari.vgrainCommand, tools: mari.vgrainTools):
    """ Eval and compute the mathematical result """
    formula = command.get_content()
    result = eval(formula)
    tools.get_logger.logInformation("Test Result " + formula + "=" + str(result))
    return {
        "result" : result,
        "formula": formula
    }

app = mari.vgrain(sys.argv)

app.run(compute);
